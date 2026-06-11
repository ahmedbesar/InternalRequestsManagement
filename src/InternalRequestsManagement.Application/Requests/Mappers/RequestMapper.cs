using System;
using System.Collections.Generic;
using InternalRequestsManagement.Requests;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests.Mappers;

public static class RequestMapper
{
    private const string Unknown = "Unknown";

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
        IReadOnlyDictionary<Guid, RequestType> typeLookup,
        IReadOnlyDictionary<Guid, OrganizationUnit> ouLookup,
        IReadOnlyDictionary<Guid, IdentityUser> userLookup)
    {
        typeLookup.TryGetValue(request.RequestTypeId, out var requestType);
        ouLookup.TryGetValue(request.OrganizationUnitId, out var ou);
        userLookup.TryGetValue(request.RequesterId, out var requester);
        IdentityUser? assignee = null;
        if (request.AssignedUserId.HasValue)
        {
            userLookup.TryGetValue(request.AssignedUserId.Value, out assignee);
        }

        return ToDto(request, requestType, ou, requester, assignee);
    }

    public static RequestStatusHistoryDto ToStatusHistoryDto(
        RequestStatusHistory history,
        IdentityUser? changedBy)
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
            RequestStatus.Draft => [RequestStatus.Submitted, RequestStatus.Cancelled],
            RequestStatus.Submitted => [RequestStatus.InProgress, RequestStatus.Rejected, RequestStatus.Cancelled],
            RequestStatus.InProgress => [RequestStatus.OnHold, RequestStatus.Resolved, RequestStatus.Cancelled],
            RequestStatus.OnHold => [RequestStatus.InProgress, RequestStatus.Cancelled, RequestStatus.Rejected],
            RequestStatus.Resolved => [RequestStatus.Closed, RequestStatus.InProgress],
            _ => []
        };
    }
}
