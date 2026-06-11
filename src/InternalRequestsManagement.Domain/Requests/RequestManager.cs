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

public class RequestManager : DomainService
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

    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public RequestManager(
        IRequestTypeRepository requestTypeRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityUserRepository userRepository,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _requestTypeRepository = requestTypeRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userRepository = userRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

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

        return Result.Ok(request);
    }

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

        return Result.Ok(request);
    }

    public Result<Request> ChangeStatus(
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

        return Result.Ok(request);
    }

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

        var requestOu = await _organizationUnitRepository.GetAsync(
            request.OrganizationUnitId, cancellationToken: cancellationToken);

        var isInSubtree = userOu.Code == requestOu.Code
                          || userOu.Code.StartsWith(requestOu.Code + ".", StringComparison.Ordinal);

        if (!isInSubtree)
            return Result.Fail<Request>(InternalRequestsManagementDomainErrorCodes.AssigneeNotInOrganizationUnit);

        request.Assign(assignedUserId);

        return Result.Ok(request);
    }

    public Result<Request> Unassign(Request request)
    {
        request.Unassign();
        return Result.Ok(request);
    }

    // ── Private helpers (non-generic Result — only signal pass/fail) ──────────

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

        var requestOu = await _organizationUnitRepository.GetAsync(organizationUnitId, cancellationToken: cancellationToken);
        var typeOu    = await _organizationUnitRepository.GetAsync(requestType.OrganizationUnitId.Value, cancellationToken: cancellationToken);

        var isAvailable = requestOu.Code == typeOu.Code
                          || requestOu.Code.StartsWith(typeOu.Code + ".", StringComparison.Ordinal);

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
