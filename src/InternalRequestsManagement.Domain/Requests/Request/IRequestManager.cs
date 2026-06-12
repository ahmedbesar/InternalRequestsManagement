using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace InternalRequestsManagement.Requests;

public interface IRequestManager
{
    Task<Result<Request>> CreateAsync(
        string title,
        string description,
        Guid requestTypeId,
        RequestPriority priority,
        Guid requesterId,
        Guid organizationUnitId,
        DateTime? dueDate,
        string? justification,
        CancellationToken cancellationToken = default);

    Task<Result<Request>> UpdateAsync(
        Request request,
        Guid organizationUnitId,
        string title,
        string description,
        Guid requestTypeId,
        RequestPriority priority,
        DateTime? dueDate,
        string? justification,
        CancellationToken cancellationToken = default);

    Task<Result<Request>> ChangeStatusAsync(
        Request request,
        RequestStatus newStatus,
        Guid changedByUserId,
        string? note,
        CancellationToken cancellationToken = default);

    Task<Result<Request>> AssignAsync(
        Request request,
        Guid assignedUserId,
        CancellationToken cancellationToken = default);

    Task<Result<Request>> UnassignAsync(
        Request request,
        CancellationToken cancellationToken = default);

    Task<Request> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Request>> GetWithHistoryAsync(Guid id, CancellationToken cancellationToken = default);

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

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> GetOpenCountAsync(IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default);

    Task<int> GetOverdueCountAsync(IReadOnlyList<Guid>? scopedOuIds, DateTime now, CancellationToken ct = default);

    Task<int> GetUnassignedCountAsync(IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default);

    Task<List<StatusCountResult>> GetCountByStatusAsync(IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default);

    Task<List<TypeCountResult>> GetCountByTypeAsync(IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default);

    Task<List<OuCountResult>> GetCountByOrganizationUnitAsync(IEnumerable<Guid> ouIds, CancellationToken ct = default);

    Task<List<AssigneeCountResult>> GetTopAssigneesAsync(IReadOnlyList<Guid>? scopedOuIds, int topCount, CancellationToken ct = default);

    Task<RequestRelationsDto> LoadRelationsAsync(
        IReadOnlyList<Request> requests,
        CancellationToken cancellationToken = default);
}
