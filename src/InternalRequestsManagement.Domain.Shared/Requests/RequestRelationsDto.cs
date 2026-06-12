using System;
using System.Collections.Generic;

namespace InternalRequestsManagement.Requests;

public sealed record RequestTypeRelationDto(
    Guid Id,
    string Name,
    bool RequiresJustification,
    bool RequiresDueDate);

public sealed record OrganizationUnitRelationDto(Guid Id, string DisplayName);

public sealed record UserRelationDto(Guid Id, string UserName);

/// <summary>
/// Holds eagerly-loaded related data required to map requests to DTOs.
/// Produced by <see cref="IRequestManager.LoadRelationsAsync"/>.
/// </summary>
public sealed record RequestRelationsDto(
    IReadOnlyDictionary<Guid, RequestTypeRelationDto> Types,
    IReadOnlyDictionary<Guid, OrganizationUnitRelationDto> OrganizationUnits,
    IReadOnlyDictionary<Guid, UserRelationDto> Users);
