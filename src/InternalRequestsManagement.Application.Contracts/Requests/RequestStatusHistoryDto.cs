using System;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests;

public sealed record RequestStatusHistoryDto(
    Guid Id,
    RequestStatus FromStatus,
    RequestStatus ToStatus,
    string? Note,
    Guid ChangedByUserId,
    string ChangedByUserName,
    DateTime ChangedAt);
