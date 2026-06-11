using System;

namespace InternalRequestsManagement.Requests;

public sealed record RequestTypeDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? OrganizationUnitId,
    string? OrganizationUnitName,
    bool RequiresJustification,
    bool RequiresDueDate,
    bool IsActive);
