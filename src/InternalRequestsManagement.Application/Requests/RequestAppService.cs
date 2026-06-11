using System;
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
    private readonly RequestMapper _requestMapper;

    public RequestAppService(
        RequestManager requestManager,
        OrganizationUnitHierarchyManager organizationUnitHierarchyManager,
        RequestMapper requestMapper)
    {
        _requestManager = requestManager;
        _organizationUnitHierarchyManager = organizationUnitHierarchyManager;
        _requestMapper = requestMapper;
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

        return new PagedResultDto<RequestDto>(totalCount,
            await _requestMapper.ToDtosAsync(items, cancellationToken));
    }

    public async Task<RequestDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var requests = await _requestManager.GetWithHistoryAsync(id, cancellationToken);
        var request = requests.FirstOrDefault();
        if (request == null)
            throw new Volo.Abp.BusinessException(InternalRequestsManagementDomainErrorCodes.RequestNotFound);

        return await _requestMapper.ToDetailDtoAsync(request, cancellationToken);
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

        return Result.Ok(await _requestMapper.ToDtoAsync(managerResult.Value, cancellationToken));
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

        return Result.Ok(await _requestMapper.ToDtoAsync(managerResult.Value, cancellationToken));
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

        return Result.Ok(await _requestMapper.ToDtoAsync(managerResult.Value, cancellationToken));
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

        return Result.Ok(await _requestMapper.ToDtoAsync(request, cancellationToken));
    }

    [Authorize(InternalRequestsManagementPermissions.Requests.Delete)]
    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _requestManager.DeleteAsync(id, cancellationToken);
}
