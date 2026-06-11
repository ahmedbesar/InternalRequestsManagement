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

namespace InternalRequestsManagement.Requests;

[Authorize(InternalRequestsManagementPermissions.Requests.Default)]
public class RequestAppService : ApplicationService, IRequestAppService
{
    private readonly RequestManager _requestManager;
    private readonly OrganizationUnitHierarchyManager _organizationUnitHierarchyManager;

    public RequestAppService(
        RequestManager requestManager,
        OrganizationUnitHierarchyManager organizationUnitHierarchyManager)
    {
        _requestManager = requestManager;
        _organizationUnitHierarchyManager = organizationUnitHierarchyManager;
    }

    public async Task<PagedResultDto<RequestDto>> GetListAsync(
        GetRequestListInput input,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = CurrentUser.Id!.Value;

        // Visibility is relevance-based: a user sees requests within their own OU
        // subtree, plus any request they created or are assigned to.
        var scopedOuIds = await _organizationUnitHierarchyManager.ResolveUserScopedOuIdsAsync(
            currentUserId, cancellationToken);

        var totalCount = await _requestManager.GetCountAsync(
            input.Search, input.Status, input.Priority, input.RequestTypeId,
            scopedOuIds, input.Scope, currentUserId, cancellationToken);

        var items = await _requestManager.GetListAsync(
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
        var requests = await _requestManager.GetWithHistoryAsync(id, cancellationToken);
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

        return Result.Ok(await MapToRequestDtoAsync(managerResult.Value, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Edit)]
    public async Task<Result<RequestDto>> UpdateAsync(
        Guid id,
        UpdateRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestManager.GetAsync(id, cancellationToken);

        var managerResult = await _requestManager.UpdateAsync(
            request, input.OrganizationUnitId, input.Title, input.Description, input.RequestTypeId,
            input.Priority, input.DueDate, input.Justification, cancellationToken);

        if (managerResult.IsFailed)
            return managerResult.ToResult<RequestDto>();

        return Result.Ok(await MapToRequestDtoAsync(managerResult.Value, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.ChangeStatus)]
    public async Task<Result<RequestDto>> ChangeStatusAsync(
        Guid id,
        ChangeRequestStatusDto input,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestManager.GetAsync(id, cancellationToken);

        var managerResult = await _requestManager.ChangeStatusAsync(
            request, input.NewStatus, CurrentUser.Id!.Value, input.Note, cancellationToken);

        if (managerResult.IsFailed)
            return managerResult.ToResult<RequestDto>();

        return Result.Ok(await MapToRequestDtoAsync(managerResult.Value, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Assign)]
    public async Task<Result<RequestDto>> AssignAsync(
        Guid id,
        AssignRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestManager.GetAsync(id, cancellationToken);

        if (!input.AssignedUserId.HasValue)
        {
            await _requestManager.UnassignAsync(request, cancellationToken);
        }
        else
        {
            var managerResult = await _requestManager.AssignAsync(request, input.AssignedUserId.Value, cancellationToken);
            if (managerResult.IsFailed)
                return managerResult.ToResult<RequestDto>();
        }

        return Result.Ok(await MapToRequestDtoAsync(request, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Delete)]
    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _requestManager.DeleteAsync(id, cancellationToken);

    // ── Private mapping helpers ───────────────────────────────────────────────

    private async Task<List<RequestDto>> MapRequestsToDtosAsync(
        List<Request> requests,
        CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
            return new List<RequestDto>();

        var relations = await _requestManager.LoadRelationsAsync(requests, cancellationToken);
        return requests
            .Select(r => RequestMapper.ToDto(r, relations.Types, relations.OrganizationUnits, relations.Users))
            .ToList();
    }

    private async Task<RequestDto> MapToRequestDtoAsync(
        Request request,
        CancellationToken cancellationToken)
    {
        var relations = await _requestManager.LoadRelationsAsync([request], cancellationToken);
        return RequestMapper.ToDto(request, relations.Types, relations.OrganizationUnits, relations.Users);
    }

    private async Task<RequestDetailDto> MapToDetailDtoAsync(
        Request request,
        CancellationToken cancellationToken)
    {
        var relations = await _requestManager.LoadRelationsAsync([request], cancellationToken);
        var requestDto = RequestMapper.ToDto(request, relations.Types, relations.OrganizationUnits, relations.Users);

        relations.Types.TryGetValue(request.RequestTypeId, out var requestType);

        var historyDtos = request.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h =>
            {
                relations.Users.TryGetValue(h.ChangedByUserId, out var changedBy);
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
