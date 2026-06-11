using System;
using System.Collections.Generic;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests;

public sealed record RequestDashboardDto(
    int OpenCount,
    int OverdueCount,
    int UnassignedCount,
    IReadOnlyList<StatusCountItemDto> ByStatus,
    IReadOnlyList<TypeCountItemDto> ByType,
    IReadOnlyList<OuCountItemDto> ByOrganizationUnit,
    IReadOnlyList<AssigneeCountItemDto> TopAssignees);

public sealed record StatusCountItemDto(RequestStatus Status, string StatusLabel, int Count);
public sealed record TypeCountItemDto(Guid RequestTypeId, string RequestTypeName, int Count);
public sealed record OuCountItemDto(Guid OrganizationUnitId, string OrganizationUnitName, int Count);
public sealed record AssigneeCountItemDto(Guid UserId, string UserName, int Count);
