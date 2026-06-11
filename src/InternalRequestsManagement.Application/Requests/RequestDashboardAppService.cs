using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.OrganizationUnits;
using InternalRequestsManagement.Permissions;
using InternalRequestsManagement.Requests.Mappers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Timing;

namespace InternalRequestsManagement.Requests;

[Authorize(InternalRequestsManagementPermissions.Requests.Default)]
public class RequestDashboardAppService : ApplicationService, IRequestDashboardAppService
{
    private readonly RequestManager _requestManager;
    private readonly OrganizationUnitHierarchyManager _organizationUnitHierarchyManager;
    private readonly IClock _clock;

    public RequestDashboardAppService(
        RequestManager requestManager,
        OrganizationUnitHierarchyManager organizationUnitHierarchyManager,
        IClock clock)
    {
        _requestManager = requestManager;
        _organizationUnitHierarchyManager = organizationUnitHierarchyManager;
        _clock = clock;
    }

    public async Task<RequestDashboardDto> GetAsync(
        Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default)
    {
        // Dashboard summaries are scoped to the current user's OU subtree.
        var scopedOuIds = await _organizationUnitHierarchyManager.ResolveUserScopedOuIdsAsync(
            CurrentUser.Id!.Value, cancellationToken);

        var now = _clock.Now;

        var openCount = await _requestManager.GetOpenCountAsync(scopedOuIds, cancellationToken);
        var overdueCount = await _requestManager.GetOverdueCountAsync(scopedOuIds, now, cancellationToken);
        var unassignedCount = await _requestManager.GetUnassignedCountAsync(scopedOuIds, cancellationToken);

        var byStatus = await _requestManager.GetCountByStatusAsync(scopedOuIds, cancellationToken);
        var byType = await _requestManager.GetCountByTypeAsync(scopedOuIds, cancellationToken);
        var topAssignees = await _requestManager.GetTopAssigneesAsync(scopedOuIds, 5, cancellationToken);

        var byStatusDtos = byStatus
            .OrderBy(x => x.Status)
            .Select(RequestDashboardMapper.ToStatusCountItem)
            .ToList();

        var byTypeDtos = byType
            .OrderByDescending(x => x.Count)
            .Select(RequestDashboardMapper.ToTypeCountItem)
            .ToList();

        var topAssigneeDtos = topAssignees
            .Select(RequestDashboardMapper.ToAssigneeCountItem)
            .ToList();

        var byOuDtos = await GetByOrganizationUnitAsync(scopedOuIds, cancellationToken);

        return RequestDashboardMapper.ToDto(
            openCount,
            overdueCount,
            unassignedCount,
            byStatusDtos,
            byTypeDtos,
            byOuDtos,
            topAssigneeDtos);
    }

    private async Task<List<OuCountItemDto>> GetByOrganizationUnitAsync(
        IReadOnlyList<Guid> scopedOuIds,
        CancellationToken cancellationToken)
    {
        // Load the full OU lookup so every OU that appears in the count results
        // has a display name available, even when the scope is wide.
        var ouLookup = await _organizationUnitHierarchyManager.GetAllLookupAsync(cancellationToken);

        IEnumerable<Guid> targetOuIds = scopedOuIds.Count > 0
            ? (IEnumerable<Guid>)scopedOuIds
            : ouLookup.Keys;

        var counts = await _requestManager.GetCountByOrganizationUnitAsync(targetOuIds, cancellationToken);

        return counts
            .Where(c => ouLookup.ContainsKey(c.OrganizationUnitId))
            .Select(c => RequestDashboardMapper.ToOuCountItem(c, ouLookup[c.OrganizationUnitId].DisplayName))
            .OrderByDescending(x => x.Count)
            .ToList();
    }
}
