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
using Volo.Abp.Identity;
using Volo.Abp.Timing;

namespace InternalRequestsManagement.Requests;

[Authorize(InternalRequestsManagementPermissions.Requests.Default)]
public class RequestDashboardAppService : ApplicationService, IRequestDashboardAppService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IClock _clock;
    private readonly OrganizationUnitSubtreeResolver _organizationUnitSubtreeResolver;

    public RequestDashboardAppService(
        IRequestRepository requestRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityUserRepository userRepository,
        IClock clock,
        OrganizationUnitSubtreeResolver organizationUnitSubtreeResolver)
    {
        _requestRepository = requestRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userRepository = userRepository;
        _clock = clock;
        _organizationUnitSubtreeResolver = organizationUnitSubtreeResolver;
    }

    public async Task<RequestDashboardDto> GetAsync(
        Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default)
    {
        // Dashboard summaries are scoped to the current user's OU subtree.
        var userOus = await _userRepository.GetOrganizationUnitsAsync(
            CurrentUser.Id!.Value, includeDetails: false, cancellationToken: cancellationToken);

        var scopedOuIds = await _organizationUnitSubtreeResolver.ResolveUserScopedOuIdsAsync(
            userOus, cancellationToken);

        var now = _clock.Now;

        var openCount = await _requestRepository.GetOpenCountAsync(scopedOuIds, cancellationToken);
        var overdueCount = await _requestRepository.GetOverdueCountAsync(scopedOuIds, now, cancellationToken);
        var unassignedCount = await _requestRepository.GetUnassignedCountAsync(scopedOuIds, cancellationToken);

        var byStatus = await _requestRepository.GetCountByStatusAsync(scopedOuIds, cancellationToken);
        var byType = await _requestRepository.GetCountByTypeAsync(scopedOuIds, cancellationToken);
        var topAssignees = await _requestRepository.GetTopAssigneesAsync(scopedOuIds, 5, cancellationToken);

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
        IReadOnlyList<Guid>? scopedOuIds,
        CancellationToken cancellationToken)
    {
        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        var ouLookup = allOus.ToDictionary(o => o.Id);

        // Use the already-resolved subtree IDs (or all OUs if admin with no scope)
        var targetOuIds = scopedOuIds != null
            ? (IEnumerable<Guid>)scopedOuIds
            : allOus.Select(o => o.Id);

        var counts = await _requestRepository.GetCountByOrganizationUnitAsync(targetOuIds, cancellationToken);

        return counts
            .Where(c => ouLookup.ContainsKey(c.OrganizationUnitId))
            .Select(c => RequestDashboardMapper.ToOuCountItem(c, ouLookup[c.OrganizationUnitId].DisplayName))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

}
