using System;

namespace InternalRequestsManagement.Requests;

public sealed record RequestDto(
    Guid Id,
    string Title,
    string Description,
    Guid RequestTypeId,
    string RequestTypeName,
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
    DateTime? LastModificationTime);
