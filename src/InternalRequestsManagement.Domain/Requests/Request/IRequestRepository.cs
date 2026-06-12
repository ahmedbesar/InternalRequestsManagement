using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace InternalRequestsManagement.Requests;

public interface IRequestRepository : IRepository<Request, Guid>
{
    Task<List<Request>> GetListAsync(
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
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? requestTypeId,
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        RequestListScope scope,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<List<Request>> GetWithHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<StatusCountResult>> GetCountByStatusAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default);

    Task<List<TypeCountResult>> GetCountByTypeAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default);

    Task<List<OuCountResult>> GetCountByOrganizationUnitAsync(
        IEnumerable<Guid> organizationUnitIds,
        CancellationToken cancellationToken = default);

    Task<List<AssigneeCountResult>> GetTopAssigneesAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        int topCount,
        CancellationToken cancellationToken = default);

    Task<int> GetOpenCountAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default);

    Task<int> GetOverdueCountAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<int> GetUnassignedCountAsync(
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        CancellationToken cancellationToken = default);
}

public sealed record StatusCountResult(RequestStatus Status, int Count);
public sealed record TypeCountResult(Guid RequestTypeId, string RequestTypeName, int Count);
public sealed record OuCountResult(Guid OrganizationUnitId, int Count);
public sealed record AssigneeCountResult(Guid UserId, string UserName, int Count);
