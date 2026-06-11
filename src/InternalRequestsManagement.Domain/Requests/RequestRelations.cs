using System;
using System.Collections.Generic;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests;

/// <summary>
/// Holds the eagerly-loaded related entities required to map a set of requests to DTOs.
/// Produced by <see cref="RequestManager.LoadRelationsAsync"/> so that the application
/// layer never needs to touch repositories directly.
/// </summary>
public sealed record RequestRelations(
    IReadOnlyDictionary<Guid, RequestType> Types,
    IReadOnlyDictionary<Guid, OrganizationUnit> OrganizationUnits,
    IReadOnlyDictionary<Guid, IdentityUser> Users);
