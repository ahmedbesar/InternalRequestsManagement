using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

public class OrganizationUnitSubtreeResolver : ITransientDependency
{
    private readonly IOrganizationUnitRepository _organizationUnitRepository;

    public OrganizationUnitSubtreeResolver(IOrganizationUnitRepository organizationUnitRepository)
    {
        _organizationUnitRepository = organizationUnitRepository;
    }

    /// <summary>
    /// Resolves the scoped OU ids for a user based on their assigned OUs.
    /// When multiple OUs are assigned, the deepest OU in the hierarchy is used
    /// so a parent assignment (e.g. company root) does not widen visibility beyond
    /// a more specific child assignment (e.g. Software Development).
    /// </summary>
    public async Task<IReadOnlyList<Guid>> ResolveUserScopedOuIdsAsync(
        IReadOnlyList<OrganizationUnit> userOrganizationUnits,
        CancellationToken cancellationToken = default)
    {
        if (userOrganizationUnits.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var rootOu = userOrganizationUnits
            .OrderByDescending(ou => ou.Code.Count(c => c == '.'))
            .ThenByDescending(ou => ou.Code.Length)
            .First();

        return await ResolveSubtreeIdsAsync(rootOu.Id, cancellationToken);
    }

    /// <summary>
    /// Returns the IDs of the given root OU and all of its descendants,
    /// resolved via <see cref="OrganizationUnit.ParentId"/> (not code prefix).
    /// </summary>
    public async Task<IReadOnlyList<Guid>> ResolveSubtreeIdsAsync(
        Guid rootOuId,
        CancellationToken cancellationToken = default)
    {
        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);

        if (allOus.All(o => o.Id != rootOuId))
        {
            return new List<Guid> { rootOuId };
        }

        var childrenByParent = allOus
            .Where(o => o.ParentId.HasValue)
            .GroupBy(o => o.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<Guid> { rootOuId };
        var queue = new Queue<Guid>();
        queue.Enqueue(rootOuId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            if (!childrenByParent.TryGetValue(parentId, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                result.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }

        return result;
    }
}
