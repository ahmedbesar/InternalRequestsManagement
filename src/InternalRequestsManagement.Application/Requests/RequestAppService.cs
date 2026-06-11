using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.OrganizationUnits;
using InternalRequestsManagement.Permissions;
using InternalRequestsManagement.Requests.Mappers;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests;

[Authorize(InternalRequestsManagementPermissions.Requests.Default)]
public class RequestAppService : ApplicationService, IRequestAppService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly RequestManager _requestManager;
    private readonly OrganizationUnitSubtreeResolver _organizationUnitSubtreeResolver;

    public RequestAppService(
        IRequestRepository requestRepository,
        IRequestTypeRepository requestTypeRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityUserRepository userRepository,
        RequestManager requestManager,
        OrganizationUnitSubtreeResolver organizationUnitSubtreeResolver)
    {
        _requestRepository = requestRepository;
        _requestTypeRepository = requestTypeRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userRepository = userRepository;
        _requestManager = requestManager;
        _organizationUnitSubtreeResolver = organizationUnitSubtreeResolver;
    }

    public async Task<PagedResultDto<RequestDto>> GetListAsync(
        GetRequestListInput input,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = CurrentUser.Id!.Value;

        // Visibility is relevance-based: a user sees requests within their own OU
        // subtree, plus any request they created or are assigned to. The OU subtree
        // is always derived from the current user's OU (no blanket "view all" bypass).
        var userOus = await _userRepository.GetOrganizationUnitsAsync(
            currentUserId, includeDetails: false, cancellationToken: cancellationToken);

        var scopedOuIds = await _organizationUnitSubtreeResolver.ResolveUserScopedOuIdsAsync(
            userOus, cancellationToken);

        var totalCount = await _requestRepository.GetCountAsync(
            input.Search, input.Status, input.Priority, input.RequestTypeId,
            scopedOuIds, input.Scope, currentUserId, cancellationToken);

        var items = await _requestRepository.GetListAsync(
            input.Search, input.Status, input.Priority, input.RequestTypeId,
            scopedOuIds, input.Scope, currentUserId,
            input.Sorting, input.MaxResultCount, input.SkipCount, cancellationToken);

        var dtos = await MapRequestsToDtosAsync(items, cancellationToken);

        return new PagedResultDto<RequestDto>(totalCount, dtos);
    }

    public async Task<RequestDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var requests = await _requestRepository.GetWithHistoryAsync(id, cancellationToken);
        var request = requests.FirstOrDefault();
        if (request == null)
        {
            throw new Volo.Abp.BusinessException(InternalRequestsManagementDomainErrorCodes.RequestNotFound);
        }

        return await MapToDetailDtoAsync(request, cancellationToken);
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Create)]
    public async Task<Result<RequestDto>> CreateAsync(
        CreateRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var managerResult = await _requestManager.CreateAsync(
            input.Title, input.Description, input.RequestTypeId, input.Priority,
            CurrentUser.Id!.Value, input.OrganizationUnitId, input.DueDate, input.Justification,
            cancellationToken);

        if (managerResult.IsFailed)
            return managerResult.ToResult<RequestDto>();

        await _requestRepository.InsertAsync(managerResult.Value, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(await MapToRequestDtoAsync(managerResult.Value, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Edit)]
    public async Task<Result<RequestDto>> UpdateAsync(
        Guid id,
        UpdateRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetAsync(id, cancellationToken: cancellationToken);

        var managerResult = await _requestManager.UpdateAsync(
            request, input.OrganizationUnitId, input.Title, input.Description, input.RequestTypeId,
            input.Priority, input.DueDate, input.Justification, cancellationToken);

        if (managerResult.IsFailed)
            return managerResult.ToResult<RequestDto>();

        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(await MapToRequestDtoAsync(request, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.ChangeStatus)]
    public async Task<Result<RequestDto>> ChangeStatusAsync(
        Guid id,
        ChangeRequestStatusDto input,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetAsync(id, cancellationToken: cancellationToken);

        var managerResult = _requestManager.ChangeStatus(
            request, input.NewStatus, CurrentUser.Id!.Value, input.Note, cancellationToken);

        if (managerResult.IsFailed)
            return managerResult.ToResult<RequestDto>();

        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(await MapToRequestDtoAsync(request, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Assign)]
    public async Task<Result<RequestDto>> AssignAsync(
        Guid id,
        AssignRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetAsync(id, cancellationToken: cancellationToken);

        if (!input.AssignedUserId.HasValue)
        {
            _requestManager.Unassign(request);
        }
        else
        {
            var managerResult = await _requestManager.AssignAsync(request, input.AssignedUserId.Value, cancellationToken);
            if (managerResult.IsFailed)
                return managerResult.ToResult<RequestDto>();
        }

        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(await MapToRequestDtoAsync(request, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Delete)]
    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _requestRepository.DeleteAsync(id, autoSave: true, cancellationToken: cancellationToken);
    }

    private async Task<List<RequestDto>> MapRequestsToDtosAsync(
        List<Request> requests,
        CancellationToken cancellationToken)
    {
        var typeIds = requests.Select(r => r.RequestTypeId).Distinct().ToList();
        var ouIds = requests.Select(r => r.OrganizationUnitId).Distinct().ToList();
        var userIds = requests.Select(r => r.RequesterId)
            .Union(requests.Where(r => r.AssignedUserId.HasValue).Select(r => r.AssignedUserId!.Value))
            .Distinct().ToList();

        var types = await _requestTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var typeLookup = types.ToDictionary(t => t.Id);

        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        var ouLookup = allOus.Where(o => ouIds.Contains(o.Id)).ToDictionary(o => o.Id);

        var users = await _userRepository.GetListAsync(cancellationToken: cancellationToken);
        var userLookup = users.Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);

        return requests.Select(r => RequestMapper.ToDto(r, typeLookup, ouLookup, userLookup)).ToList();
    }

    private async Task<RequestDto> MapToRequestDtoAsync(
        Request request,
        CancellationToken cancellationToken)
    {
        var requestType = await _requestTypeRepository.FindAsync(request.RequestTypeId, cancellationToken: cancellationToken);
        var ou = await _organizationUnitRepository.FindAsync(request.OrganizationUnitId, cancellationToken: cancellationToken);
        var requester = await _userRepository.FindAsync(request.RequesterId, cancellationToken: cancellationToken);
        IdentityUser? assignee = null;
        if (request.AssignedUserId.HasValue)
        {
            assignee = await _userRepository.FindAsync(request.AssignedUserId.Value, cancellationToken: cancellationToken);
        }

        return RequestMapper.ToDto(request, requestType, ou, requester, assignee);
    }

    private async Task<RequestDetailDto> MapToDetailDtoAsync(
        Request request,
        CancellationToken cancellationToken)
    {
        var requestDto = await MapToRequestDtoAsync(request, cancellationToken);
        var requestType = await _requestTypeRepository.FindAsync(request.RequestTypeId, cancellationToken: cancellationToken);

        var allUsers = await _userRepository.GetListAsync(cancellationToken: cancellationToken);
        var userLookup = allUsers.ToDictionary(u => u.Id);

        var historyDtos = request.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h =>
            {
                userLookup.TryGetValue(h.ChangedByUserId, out var changedBy);
                return RequestMapper.ToStatusHistoryDto(h, changedBy);
            })
            .ToList();

        return RequestMapper.ToDetailDto(
            requestDto,
            requestType?.RequiresJustification ?? false,
            requestType?.RequiresDueDate ?? false,
            historyDtos,
            RequestMapper.GetAllowedNextStatuses(request.Status));
    }
}
