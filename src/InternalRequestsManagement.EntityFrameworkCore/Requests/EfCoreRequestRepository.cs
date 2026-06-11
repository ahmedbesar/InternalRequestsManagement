using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace InternalRequestsManagement.Requests;

public class EfCoreRequestRepository :
    EfCoreRepository<InternalRequestsManagementDbContext, Request, Guid>,
    IRequestRepository
{
    public EfCoreRequestRepository(IDbContextProvider<InternalRequestsManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Request>> GetListAsync(
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? requestTypeId,
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        RequestListScope scope,
        Guid currentUserId,
        string? sorting,
        int maxResultCount,
        int skipCount,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;

        var query = BuildQuery(dbContext, search, status, priority, requestTypeId,
            scopedOrganizationUnitIds, scope, currentUserId, now);

        if (!string.IsNullOrWhiteSpace(sorting))
            query = query.OrderBy(sorting);
        else
            query = query.OrderByDescending(r => r.CreationTime);

        return await query.Skip(skipCount).Take(maxResultCount).ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? requestTypeId,
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        RequestListScope scope,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;

        var query = BuildQuery(dbContext, search, status, priority, requestTypeId,
            scopedOrganizationUnitIds, scope, currentUserId, now);

        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<List<Request>> GetWithHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Requests
            .Include(r => r.StatusHistory)
            .Where(r => r.Id == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StatusCountResult>> GetCountByStatusAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = ApplyOuFilter(dbContext.Requests.AsQueryable(), scopedOrganizationUnitIds);

        return await query
            .GroupBy(r => r.Status)
            .Select(g => new StatusCountResult(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TypeCountResult>> GetCountByTypeAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = ApplyOuFilter(dbContext.Requests.AsQueryable(), scopedOrganizationUnitIds);

        return await query
            .Join(dbContext.RequestTypes, r => r.RequestTypeId, t => t.Id,
                (r, t) => new { r.RequestTypeId, t.Name })
            .GroupBy(x => new { x.RequestTypeId, x.Name })
            .Select(g => new TypeCountResult(g.Key.RequestTypeId, g.Key.Name, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OuCountResult>> GetCountByOrganizationUnitAsync(
        IEnumerable<Guid> organizationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var ouIdList = organizationUnitIds.ToList();

        return await dbContext.Requests
            .Where(r => ouIdList.Contains(r.OrganizationUnitId))
            .GroupBy(r => r.OrganizationUnitId)
            .Select(g => new OuCountResult(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AssigneeCountResult>> GetTopAssigneesAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = ApplyOuFilter(dbContext.Requests.AsQueryable(), scopedOrganizationUnitIds)
            .Where(r => r.AssignedUserId.HasValue);

        var grouped = await query
            .GroupBy(r => r.AssignedUserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        var userIds = grouped.Select(g => g.UserId).ToList();
        var users = await dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync(cancellationToken);

        return grouped
            .Join(users, g => g.UserId, u => u.Id,
                (g, u) => new AssigneeCountResult(g.UserId, u.UserName, g.Count))
            .ToList();
    }

    public async Task<int> GetOpenCountAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var terminalStatuses = RequestConsts.TerminalStatuses;

        var query = ApplyOuFilter(dbContext.Requests.AsQueryable(), scopedOrganizationUnitIds)
            .Where(r => !terminalStatuses.Contains(r.Status));

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetOverdueCountAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var terminalStatuses = RequestConsts.TerminalStatuses;

        var query = ApplyOuFilter(dbContext.Requests.AsQueryable(), scopedOrganizationUnitIds)
            .Where(r => r.DueDate.HasValue && r.DueDate.Value < now && !terminalStatuses.Contains(r.Status));

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetUnassignedCountAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var terminalStatuses = RequestConsts.TerminalStatuses;

        var query = ApplyOuFilter(dbContext.Requests.AsQueryable(), scopedOrganizationUnitIds)
            .Where(r => r.AssignedUserId == null && !terminalStatuses.Contains(r.Status));

        return await query.CountAsync(cancellationToken);
    }

    // --- helpers ----------------------------------------------------------------

    private static IQueryable<Request> ApplyOuFilter(
        IQueryable<Request> query,
        IReadOnlyList<Guid>? scopedOrganizationUnitIds)
    {
        // A non-null list always filters - an empty list intentionally yields no
        // results (e.g. a user with no Organization Unit). null means no scoping.
        if (scopedOrganizationUnitIds != null)
        {
            query = query.Where(r => scopedOrganizationUnitIds.Contains(r.OrganizationUnitId));
        }

        return query;
    }

    private static IQueryable<Request> BuildQuery(
        InternalRequestsManagementDbContext dbContext,
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? requestTypeId,
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        RequestListScope scope,
        Guid currentUserId,
        DateTime now)
    {
        var query = dbContext.Requests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                r.Title.Contains(search) ||
                r.Description.Contains(search));
        }

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        if (requestTypeId.HasValue)
            query = query.Where(r => r.RequestTypeId == requestTypeId.Value);

        var terminalStatuses = RequestConsts.TerminalStatuses;
        var ouIds = scopedOrganizationUnitIds ?? new List<Guid>();

        query = scope switch
        {
            RequestListScope.Mine => query.Where(r => r.RequesterId == currentUserId),

            RequestListScope.AssignedToMe => query.Where(r => r.AssignedUserId == currentUserId),

            RequestListScope.MyOrganizationUnit => query.Where(r => ouIds.Contains(r.OrganizationUnitId)),

            RequestListScope.Unassigned => query.Where(r =>
                ouIds.Contains(r.OrganizationUnitId) &&
                r.AssignedUserId == null &&
                !terminalStatuses.Contains(r.Status)),

            RequestListScope.Overdue => query.Where(r =>
                (ouIds.Contains(r.OrganizationUnitId) ||
                 r.RequesterId == currentUserId ||
                 r.AssignedUserId == currentUserId) &&
                r.DueDate.HasValue && r.DueDate.Value < now &&
                !terminalStatuses.Contains(r.Status)),

            // All: relevance union - OU subtree OR my own OR assigned to me.
            _ => query.Where(r =>
                ouIds.Contains(r.OrganizationUnitId) ||
                r.RequesterId == currentUserId ||
                r.AssignedUserId == currentUserId),
        };

        return query;
    }
}
