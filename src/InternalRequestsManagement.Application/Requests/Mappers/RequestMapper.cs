using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests.Mappers;

/// <summary>
/// Handles all Request → DTO conversion.
/// The async overloads own the <see cref="IRequestManager.LoadRelationsAsync"/> call so
/// the application service never has to touch relation-loading directly.
/// </summary>
public class RequestMapper : ITransientDependency
{
    private const string Unknown = "Unknown";

    private readonly IRequestManager _requestManager;

    public RequestMapper(IRequestManager requestManager)
    {
        _requestManager = requestManager;
    }

    // ── Async overloads (load relations internally) ───────────────────────────

    /// <summary>Maps a batch of requests to DTOs, loading all required relations in one round-trip.</summary>
    public async Task<List<RequestDto>> ToDtosAsync(
        List<Request> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
            return [];

        var relations = await _requestManager.LoadRelationsAsync(requests, cancellationToken);
        return requests
            .Select(r => ToDto(r, relations.Types, relations.OrganizationUnits, relations.Users))
            .ToList();
    }

    /// <summary>Maps a single request to a DTO, loading all required relations.</summary>
    public async Task<RequestDto> ToDtoAsync(
        Request request,
        CancellationToken cancellationToken = default)
    {
        var relations = await _requestManager.LoadRelationsAsync([request], cancellationToken);
        return ToDto(request, relations.Types, relations.OrganizationUnits, relations.Users);
    }

    /// <summary>Maps a request (with its status history already populated) to a detail DTO,
    /// loading all required relations including history users.</summary>
    public async Task<RequestDetailDto> ToDetailDtoAsync(
        Request request,
        CancellationToken cancellationToken = default)
    {
        var relations = await _requestManager.LoadRelationsAsync([request], cancellationToken);
        var requestDto = ToDto(request, relations.Types, relations.OrganizationUnits, relations.Users);

        relations.Types.TryGetValue(request.RequestTypeId, out var requestType);

        var historyDtos = request.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h =>
            {
                relations.Users.TryGetValue(h.ChangedByUserId, out var changedBy);
                return ToStatusHistoryDto(h, changedBy);
            })
            .ToList();

        return ToDetailDto(
            requestDto,
            requestType?.RequiresJustification ?? false,
            requestType?.RequiresDueDate ?? false,
            historyDtos,
            GetAllowedNextStatuses(request.Status));
    }

    // ── Pure synchronous mapping (no I/O — usable when relations are already loaded) ──

    public static RequestDto ToDto(
        Request request,
        RequestType? requestType,
        OrganizationUnit? organizationUnit,
        IdentityUser? requester,
        IdentityUser? assignee)
    {
        return new RequestDto(
            request.Id,
            request.Title,
            request.Description,
            request.RequestTypeId,
            requestType?.Name ?? Unknown,
            request.Priority,
            request.Status,
            request.RequesterId,
            requester?.UserName ?? Unknown,
            request.AssignedUserId,
            assignee?.UserName,
            request.DueDate,
            request.OrganizationUnitId,
            organizationUnit?.DisplayName ?? Unknown,
            request.Justification,
            request.CreationTime,
            request.LastModificationTime);
    }

    public static RequestDto ToDto(
        Request request,
        IReadOnlyDictionary<Guid, RequestTypeRelationDto> typeLookup,
        IReadOnlyDictionary<Guid, OrganizationUnitRelationDto> ouLookup,
        IReadOnlyDictionary<Guid, UserRelationDto> userLookup)
    {
        typeLookup.TryGetValue(request.RequestTypeId, out var requestType);
        ouLookup.TryGetValue(request.OrganizationUnitId, out var ou);
        userLookup.TryGetValue(request.RequesterId, out var requester);
        UserRelationDto? assignee = null;
        if (request.AssignedUserId.HasValue)
            userLookup.TryGetValue(request.AssignedUserId.Value, out assignee);

        return new RequestDto(
            request.Id,
            request.Title,
            request.Description,
            request.RequestTypeId,
            requestType?.Name ?? Unknown,
            request.Priority,
            request.Status,
            request.RequesterId,
            requester?.UserName ?? Unknown,
            request.AssignedUserId,
            assignee?.UserName,
            request.DueDate,
            request.OrganizationUnitId,
            ou?.DisplayName ?? Unknown,
            request.Justification,
            request.CreationTime,
            request.LastModificationTime);
    }

    public static RequestStatusHistoryDto ToStatusHistoryDto(
        RequestStatusHistory history,
        UserRelationDto? changedBy)
    {
        return new RequestStatusHistoryDto(
            history.Id,
            history.FromStatus,
            history.ToStatus,
            history.Note,
            history.ChangedByUserId,
            changedBy?.UserName ?? Unknown,
            history.ChangedAt);
    }

    public static RequestDetailDto ToDetailDto(
        RequestDto requestDto,
        bool requiresJustification,
        bool requiresDueDate,
        IReadOnlyList<RequestStatusHistoryDto> history,
        IReadOnlyList<RequestStatus> allowedNextStatuses)
    {
        return new RequestDetailDto(
            requestDto.Id,
            requestDto.Title,
            requestDto.Description,
            requestDto.RequestTypeId,
            requestDto.RequestTypeName,
            requiresJustification,
            requiresDueDate,
            requestDto.Priority,
            requestDto.Status,
            requestDto.RequesterId,
            requestDto.RequesterName,
            requestDto.AssignedUserId,
            requestDto.AssignedUserName,
            requestDto.DueDate,
            requestDto.OrganizationUnitId,
            requestDto.OrganizationUnitName,
            requestDto.Justification,
            requestDto.CreationTime,
            requestDto.LastModificationTime,
            history,
            allowedNextStatuses);
    }

    public static List<RequestStatus> GetAllowedNextStatuses(RequestStatus current)
    {
        return current switch
        {
            RequestStatus.Draft      => [RequestStatus.Submitted, RequestStatus.Cancelled],
            RequestStatus.Submitted  => [RequestStatus.InProgress, RequestStatus.Rejected, RequestStatus.Cancelled],
            RequestStatus.InProgress => [RequestStatus.OnHold, RequestStatus.Resolved, RequestStatus.Cancelled],
            RequestStatus.OnHold     => [RequestStatus.InProgress, RequestStatus.Cancelled, RequestStatus.Rejected],
            RequestStatus.Resolved   => [RequestStatus.Closed, RequestStatus.InProgress],
            _                        => []
        };
    }
}
