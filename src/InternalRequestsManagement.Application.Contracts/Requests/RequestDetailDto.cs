using System;
using System.Collections.Generic;

namespace InternalRequestsManagement.Requests;

public sealed record RequestDetailDto(
    Guid Id,
    string Title,
    string Description,
    Guid RequestTypeId,
    string RequestTypeName,
    bool RequestTypeRequiresJustification,
    bool RequestTypeRequiresDueDate,
    RequestPriority Priority,
    RequestStatus Status,
    Guid RequesterId,
    string RequesterName,
    Guid? AssignedUserId,
    string? AssignedUserName,
    DateTime? DueDate,
    Guid OrganizationUnitId,
    string OrganizationUnitName,
    string? Justification,
    DateTime CreationTime,
    DateTime? LastModificationTime,
    IReadOnlyList<RequestStatusHistoryDto> StatusHistory,
    IReadOnlyList<RequestStatus> AllowedNextStatuses);
