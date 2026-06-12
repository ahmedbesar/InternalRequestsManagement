using System;
using Volo.Abp.Domain.Entities;

namespace InternalRequestsManagement.Requests;

public class RequestStatusHistory : Entity<Guid>
{
    public Guid RequestId { get; private set; }

    public RequestStatus FromStatus { get; private set; }

    public RequestStatus ToStatus { get; private set; }

    public string? Note { get; private set; }

    public Guid ChangedByUserId { get; private set; }

    public DateTime ChangedAt { get; private set; }

    private RequestStatusHistory() { }

    internal static RequestStatusHistory Create(
        Guid id,
        Guid requestId,
        RequestStatus fromStatus,
        RequestStatus toStatus,
        Guid changedByUserId,
        DateTime changedAt,
        string? note = null)
    {
        return new RequestStatusHistory
        {
            Id = id,
            RequestId = requestId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            ChangedAt = changedAt,
            Note = note
        };
    }
}
