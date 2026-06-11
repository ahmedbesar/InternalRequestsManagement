using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

/// <summary>
/// Domain service that owns all OU hierarchy operations: user-scoped subtree resolution,
/// single OU lookup, ancestor path building, and batch ID→OU mapping.
/// Uses ABP's <see cref="OrganizationUnitManager.FindChildrenAsync"/> for subtree traversal
/// instead of manual BFS so the logic stays in sync with ABP's own tree management.
/// </summary>
public class OrganizationUnitHierarchyManager : DomainService
{
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IIdentityUserRepository _userRepository;

    public OrganizationUnitHierarchyManager(
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityUserRepository userRepository)
    {
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Returns the IDs of all OUs visible to <paramref name="userId"/>: the deepest OU
    /// they are assigned to plus all its descendants.  When a user has multiple OU
    /// assignments the deepest one takes precedence so a parent-level assignment cannot
    /// silently widen visibility beyond a more specific child assignment.
    /// </summary>
    public async Task<IReadOnlyList<Guid>> ResolveUserScopedOuIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userOus = await _userRepository.GetOrganizationUnitsAsync(
            userId, includeDetails: false, cancellationToken: cancellationToken);

        if (userOus.Count == 0)
            return Array.Empty<Guid>();

        // Pick deepest OU (most dots = deepest level; tie-break by code length)
        var rootOu = userOus
            .OrderByDescending(ou => ou.Code.Count(c => c == '.'))
            .ThenByDescending(ou => ou.Code.Length)
            .First();

        // Use ABP's recursive FindChildrenAsync instead of a manual BFS
        var descendants = await _organizationUnitManager.FindChildrenAsync(
            rootOu.Id, recursive: true);

        var ids = new List<Guid>(descendants.Count + 1) { rootOu.Id };
        ids.AddRange(descendants.Select(d => d.Id));
        return ids;
    }

    /// <summary>Returns a single OU by id; throws if not found.</summary>
    public Task<OrganizationUnit> GetAsync(
        Guid ouId,
        CancellationToken cancellationToken = default)
        => _organizationUnitRepository.GetAsync(ouId, cancellationToken: cancellationToken);

    /// <summary>
    /// Walks from the given OU up to the root and returns the path ordered root→leaf.
    /// Used to pre-populate the OU breadcrumb in the UI.
    /// </summary>
    public async Task<List<OrganizationUnit>> GetPathAsync(
        Guid ouId,
        CancellationToken cancellationToken = default)
    {
        var path = new List<OrganizationUnit>();
        var current = await _organizationUnitRepository.GetAsync(ouId, cancellationToken: cancellationToken);

        while (true)
        {
            path.Insert(0, current);
            if (!current.ParentId.HasValue)
                break;
            current = await _organizationUnitRepository.GetAsync(
                current.ParentId.Value, cancellationToken: cancellationToken);
        }

        return path;
    }

    /// <summary>
    /// Returns a dictionary of id → OU for the given set of ids.
    /// Useful for resolving OU display names without loading the whole tree.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, OrganizationUnit>> GetLookupAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet();
        if (idSet.Count == 0)
            return new Dictionary<Guid, OrganizationUnit>();

        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        return allOus.Where(o => idSet.Contains(o.Id)).ToDictionary(o => o.Id);
    }

    /// <summary>Returns a dictionary of id → OU for every OU in the system.</summary>
    public async Task<IReadOnlyDictionary<Guid, OrganizationUnit>> GetAllLookupAsync(
        CancellationToken cancellationToken = default)
    {
        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        return allOus.ToDictionary(o => o.Id);
    }
}
