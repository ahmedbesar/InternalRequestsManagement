using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Timing;

namespace InternalRequestsManagement.Requests;

public class RequestManager : DomainService, IRequestManager
{
    private static readonly Dictionary<RequestStatus, RequestStatus[]> AllowedTransitions = new()
    {
        [RequestStatus.Draft]      = [RequestStatus.Submitted, RequestStatus.Cancelled],
        [RequestStatus.Submitted]  = [RequestStatus.InProgress, RequestStatus.Rejected, RequestStatus.Cancelled],
        [RequestStatus.InProgress] = [RequestStatus.OnHold, RequestStatus.Resolved, RequestStatus.Cancelled],
        [RequestStatus.OnHold]     = [RequestStatus.InProgress, RequestStatus.Cancelled, RequestStatus.Rejected],
        [RequestStatus.Resolved]   = [RequestStatus.Closed, RequestStatus.InProgress],
        [RequestStatus.Closed]     = [],
        [RequestStatus.Cancelled]  = [],
        [RequestStatus.Rejected]   = [],
    };

    private readonly IRequestRepository _requestRepository;
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public RequestManager(
        IRequestRepository requestRepository,
        IRequestTypeRepository requestTypeRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityUserRepository userRepository,
        OrganizationUnitManager organizationUnitManager,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _requestRepository = requestRepository;
        _requestTypeRepository = requestTypeRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userRepository = userRepository;
        _organizationUnitManager = organizationUnitManager;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    // ── Business logic ────────────────────────────────────────────────────────

    /// <summary>Validates business rules (active type valid for the OU, justification and due-date requirements),
    /// builds a new request and persists it.</summary>
    public async Task<Result<Request>> CreateAsync(
        string title,
        string description,
        Guid requestTypeId,
        RequestPriority priority,
        Guid requesterId,
        Guid organizationUnitId,
        DateTime? dueDate,
        string? justification,
        CancellationToken cancellationToken = default)
    {
        var requestType = await GetActiveRequestTypeAsync(requestTypeId, cancellationToken);

        var ouResult = await ValidateRequestTypeForOuAsync(requestType, organizationUnitId, cancellationToken);
        if (ouResult.IsFailed) return ouResult.ToResult<Request>();

        var justResult = ValidateJustification(requestType, priority, justification);
        if (justResult.IsFailed) return justResult.ToResult<Request>();

        var dateResult = ValidateDueDate(requestType, priority, dueDate);
        if (dateResult.IsFailed) return dateResult.ToResult<Request>();

        var request = Request.Create(
            _guidGenerator.Create(),
            CurrentTenant.Id,
            title, description, requestTypeId, priority,
            requesterId, organizationUnitId, dueDate, justification);

        await _requestRepository.InsertAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(request);
    }

    /// <summary>Re-validates business rules and updates an existing request's details; rejected if the request is in a terminal status.
    /// Persists the change.</summary>
    public async Task<Result<Request>> UpdateAsync(
        Request request,
        Guid organizationUnitId,
        string title,
        string description,
        Guid requestTypeId,
        RequestPriority priority,
        DateTime? dueDate,
        string? justification,
        CancellationToken cancellationToken = default)
    {
        if (RequestConsts.IsTerminal(request.Status))
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.RequestAlreadyInTerminalStatus);

        var requestType = await GetActiveRequestTypeAsync(requestTypeId, cancellationToken);

        var ouResult = await ValidateRequestTypeForOuAsync(requestType, organizationUnitId, cancellationToken);
        if (ouResult.IsFailed) return ouResult.ToResult<Request>();

        var justResult = ValidateJustification(requestType, priority, justification);
        if (justResult.IsFailed) return justResult.ToResult<Request>();

        var dateResult = ValidateDueDate(requestType, priority, dueDate);
        if (dateResult.IsFailed) return dateResult.ToResult<Request>();

        request.UpdateDetails(organizationUnitId, title, description, requestTypeId, priority, dueDate, justification);

        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(request);
    }

    /// <summary>Transitions the request to a new status, enforcing the allowed transition map
    /// and requiring a note for statuses that mandate one.  Persists the change.</summary>
    public async Task<Result<Request>> ChangeStatusAsync(
        Request request,
        RequestStatus newStatus,
        Guid changedByUserId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (RequestConsts.IsTerminal(request.Status))
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.RequestAlreadyInTerminalStatus);

        if (!IsTransitionAllowed(request.Status, newStatus))
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.InvalidStatusTransition);

        if (RequestConsts.RequiresNote(newStatus) && string.IsNullOrWhiteSpace(note))
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.StatusNoteRequired);

        request.ChangeStatus(_guidGenerator.Create(), newStatus, changedByUserId, _clock.Now, note);

        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(request);
    }

    /// <summary>Assigns the request to a user, ensuring the user belongs to the request's OU subtree.
    /// Persists the change.</summary>
    public async Task<Result<Request>> AssignAsync(
        Request request,
        Guid assignedUserId,
        CancellationToken cancellationToken = default)
    {
        var userOus = await _userRepository.GetOrganizationUnitsAsync(
            assignedUserId, includeDetails: false, cancellationToken: cancellationToken);
        var userOu = userOus.FirstOrDefault();

        if (userOu == null)
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.AssigneeNotInOrganizationUnit);

        var requestOuCode = await _organizationUnitManager.GetCodeOrDefaultAsync(request.OrganizationUnitId);

        if (requestOuCode == null)
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.AssigneeNotInOrganizationUnit);

        var isInSubtree = userOu.Code == requestOuCode
                          || userOu.Code.StartsWith(requestOuCode + ".", StringComparison.Ordinal);

        if (!isInSubtree)
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.AssigneeNotInOrganizationUnit);

        request.Assign(assignedUserId);

        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);

        return Result.Ok(request);
    }

    /// <summary>Clears the request's current assignee and persists the change.</summary>
    public async Task<Result<Request>> UnassignAsync(
        Request request,
        CancellationToken cancellationToken = default)
    {
        request.Unassign();
        await _requestRepository.UpdateAsync(request, autoSave: true, cancellationToken: cancellationToken);
        return Result.Ok(request);
    }

    // ── Query pass-throughs ───────────────────────────────────────────────────

    /// <summary>Returns the request by id; throws if not found.</summary>
    public Task<Request> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => _requestRepository.GetAsync(id, cancellationToken: cancellationToken);

    /// <summary>Returns the request together with its full status history; the list contains one item or is empty.</summary>
    public Task<List<Request>> GetWithHistoryAsync(Guid id, CancellationToken cancellationToken = default)
        => _requestRepository.GetWithHistoryAsync(id, cancellationToken);

    /// <summary>Returns a filtered, sorted page of requests.</summary>
    public Task<List<Request>> GetListAsync(
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
        => _requestRepository.GetListAsync(
            search, status, priority, requestTypeId,
            scopedOrganizationUnitIds, scope, currentUserId,
            sorting, maxResultCount, skipCount, cancellationToken);

    /// <summary>Returns the total count matching the same filter used by <see cref="GetListAsync"/>.</summary>
    public Task<long> GetCountAsync(
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? requestTypeId,
        IReadOnlyList<Guid>? scopedOrganizationUnitIds,
        RequestListScope scope,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
        => _requestRepository.GetCountAsync(
            search, status, priority, requestTypeId,
            scopedOrganizationUnitIds, scope, currentUserId, cancellationToken);

    /// <summary>Deletes the request by id.</summary>
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _requestRepository.DeleteAsync(id, autoSave: true, cancellationToken: cancellationToken);

    // ── Dashboard stat pass-throughs ──────────────────────────────────────────

    public Task<int> GetOpenCountAsync(
        IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default)
        => _requestRepository.GetOpenCountAsync(scopedOuIds, ct);

    public Task<int> GetOverdueCountAsync(
        IReadOnlyList<Guid>? scopedOuIds, DateTime now, CancellationToken ct = default)
        => _requestRepository.GetOverdueCountAsync(scopedOuIds, now, ct);

    public Task<int> GetUnassignedCountAsync(
        IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default)
        => _requestRepository.GetUnassignedCountAsync(scopedOuIds, ct);

    public Task<List<StatusCountResult>> GetCountByStatusAsync(
        IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default)
        => _requestRepository.GetCountByStatusAsync(scopedOuIds, ct);

    public Task<List<TypeCountResult>> GetCountByTypeAsync(
        IReadOnlyList<Guid>? scopedOuIds, CancellationToken ct = default)
        => _requestRepository.GetCountByTypeAsync(scopedOuIds, ct);

    public Task<List<OuCountResult>> GetCountByOrganizationUnitAsync(
        IEnumerable<Guid> ouIds, CancellationToken ct = default)
        => _requestRepository.GetCountByOrganizationUnitAsync(ouIds, ct);

    public Task<List<AssigneeCountResult>> GetTopAssigneesAsync(
        IReadOnlyList<Guid>? scopedOuIds, int topCount, CancellationToken ct = default)
        => _requestRepository.GetTopAssigneesAsync(scopedOuIds, topCount, ct);

    // ── Relation loading ──────────────────────────────────────────────────────

    /// <summary>
    /// Eagerly loads the type, OU and user entities referenced by <paramref name="requests"/>
    /// (including status-history users when history is populated) and returns them as
    /// look-up dictionaries so the application layer can call <c>RequestMapper</c>
    /// without touching repositories directly.
    /// </summary>
    public async Task<RequestRelationsDto> LoadRelationsAsync(
        IReadOnlyList<Request> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0)
        {
            return new RequestRelationsDto(
                new Dictionary<Guid, RequestTypeRelationDto>(),
                new Dictionary<Guid, OrganizationUnitRelationDto>(),
                new Dictionary<Guid, UserRelationDto>());
        }

        var ouIds = requests.Select(r => r.OrganizationUnitId).Distinct().ToList();

        var userIds = requests.Select(r => r.RequesterId)
            .Union(requests.Where(r => r.AssignedUserId.HasValue).Select(r => r.AssignedUserId!.Value))
            .Union(requests.SelectMany(r => r.StatusHistory.Select(h => h.ChangedByUserId)))
            .Distinct()
            .ToHashSet();

        var allTypes = await _requestTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var typeLookup = allTypes.ToDictionary(
            t => t.Id,
            t => new RequestTypeRelationDto(t.Id, t.Name, t.RequiresJustification, t.RequiresDueDate));

        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        var ouLookup = allOus
            .Where(o => ouIds.Contains(o.Id))
            .ToDictionary(o => o.Id, o => new OrganizationUnitRelationDto(o.Id, o.DisplayName));

        var allUsers = await _userRepository.GetListAsync(cancellationToken: cancellationToken);
        var userLookup = allUsers
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => new UserRelationDto(u.Id, u.UserName));

        return new RequestRelationsDto(typeLookup, ouLookup, userLookup);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<RequestType> GetActiveRequestTypeAsync(
        Guid requestTypeId, CancellationToken cancellationToken)
    {
        var requestType = await _requestTypeRepository.FindAsync(requestTypeId, cancellationToken: cancellationToken);
        if (requestType == null || !requestType.IsActive)
            throw new BusinessException(InternalRequestsManagementDomainErrorCodes.RequestTypeNotFound);
        return requestType;
    }

    private async Task<Result> ValidateRequestTypeForOuAsync(
        RequestType requestType, Guid organizationUnitId, CancellationToken cancellationToken)
    {
        if (requestType.OrganizationUnitId == null)
            return Result.Ok();

        var requestOuCode = await _organizationUnitManager.GetCodeOrDefaultAsync(organizationUnitId);
        var typeOuCode = await _organizationUnitManager.GetCodeOrDefaultAsync(requestType.OrganizationUnitId.Value);

        if (requestOuCode == null || typeOuCode == null)
            return Result.Fail(InternalRequestsManagementDomainErrorCodes.RequestTypeNotAvailableForOrganizationUnit);

        var isAvailable = requestOuCode == typeOuCode
                          || requestOuCode.StartsWith(typeOuCode + ".", StringComparison.Ordinal);

        return isAvailable
            ? Result.Ok()
            : Result.Fail(InternalRequestsManagementDomainErrorCodes.RequestTypeNotAvailableForOrganizationUnit);
    }

    private static Result ValidateJustification(
        RequestType requestType, RequestPriority priority, string? justification)
    {
        bool required = requestType.RequiresJustification || priority == RequestPriority.Critical;
        return required && string.IsNullOrWhiteSpace(justification)
            ? Result.Fail(InternalRequestsManagementDomainErrorCodes.JustificationRequired)
            : Result.Ok();
    }

    private static Result ValidateDueDate(
        RequestType requestType, RequestPriority priority, DateTime? dueDate)
    {
        bool required = requestType.RequiresDueDate || priority == RequestPriority.Critical;
        return required && !dueDate.HasValue
            ? Result.Fail(InternalRequestsManagementDomainErrorCodes.DueDateRequired)
            : Result.Ok();
    }

    private static bool IsTransitionAllowed(RequestStatus from, RequestStatus to)
        => AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
