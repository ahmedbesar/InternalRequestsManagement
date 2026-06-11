using System.ComponentModel.DataAnnotations;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests;

public sealed record ChangeRequestStatusDto(
    [Required]
    RequestStatus NewStatus,

    [StringLength(RequestConsts.MaxStatusNoteLength)]
    string? Note);
