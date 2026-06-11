using System;
using System.ComponentModel.DataAnnotations;

namespace InternalRequestsManagement.Requests;

public sealed record AssignRequestDto([Required] Guid? AssignedUserId);
