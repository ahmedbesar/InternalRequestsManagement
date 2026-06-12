using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace InternalRequestsManagement.Requests;

public class Request : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public Guid RequestTypeId { get; private set; }

    public RequestPriority Priority { get; private set; }

    public RequestStatus Status { get; private set; }

    public Guid RequesterId { get; private set; }

    public Guid? AssignedUserId { get; private set; }

    public DateTime? DueDate { get; private set; }

    public Guid OrganizationUnitId { get; private set; }

    public string? Justification { get; private set; }

    private readonly List<RequestStatusHistory> _statusHistory = [];
    public IReadOnlyCollection<RequestStatusHistory> StatusHistory => new ReadOnlyCollection<RequestStatusHistory>(_statusHistory);

    private Request() { }

    internal static Request Create(
        Guid id,
        Guid? tenantId,
        string title,
        string description,
        Guid requestTypeId,
        RequestPriority priority,
        Guid requesterId,
        Guid organizationUnitId,
        DateTime? dueDate,
        string? justification)
    {
        return new Request
        {
            Id = id,
            TenantId = tenantId,
            Title = Check.NotNullOrWhiteSpace(title, nameof(title), RequestConsts.MaxTitleLength, RequestConsts.MinTitleLength),
            Description = Check.NotNullOrWhiteSpace(description, nameof(description), RequestConsts.MaxDescriptionLength),
            RequestTypeId = requestTypeId,
            Priority = priority,
            Status = RequestStatus.Draft,
            RequesterId = requesterId,
            OrganizationUnitId = organizationUnitId,
            DueDate = dueDate,
            Justification = justification
        };
    }

    internal void UpdateDetails(
        Guid organizationUnitId,
        string title,
        string description,
        Guid requestTypeId,
        RequestPriority priority,
        DateTime? dueDate,
        string? justification)
    {
        OrganizationUnitId = organizationUnitId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), RequestConsts.MaxTitleLength, RequestConsts.MinTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), RequestConsts.MaxDescriptionLength);
        RequestTypeId = requestTypeId;
        Priority = priority;
        DueDate = dueDate;
        Justification = justification;
    }

    internal void ChangeStatus(
        Guid historyId,
        RequestStatus newStatus,
        Guid changedByUserId,
        DateTime changedAt,
        string? note)
    {
        var history = RequestStatusHistory.Create(
            historyId,
            Id,
            Status,
            newStatus,
            changedByUserId,
            changedAt,
            note);

        _statusHistory.Add(history);
        Status = newStatus;
    }

    internal void Assign(Guid userId)
    {
        AssignedUserId = userId;
    }

    internal void Unassign()
    {
        AssignedUserId = null;
    }

    public bool IsOverdue(DateTime now) =>
        DueDate.HasValue && DueDate.Value < now && !RequestConsts.IsTerminal(Status);

    public bool IsUnassigned() => AssignedUserId == null && !RequestConsts.IsTerminal(Status);
}
