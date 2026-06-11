using System;
using System.ComponentModel.DataAnnotations;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests;

public sealed record UpdateRequestDto(
    [Required]
    Guid OrganizationUnitId,

    [Required][StringLength(RequestConsts.MaxTitleLength, MinimumLength = RequestConsts.MinTitleLength)]
    string Title,

    [Required][StringLength(RequestConsts.MaxDescriptionLength)]
    string Description,

    [Required]
    Guid RequestTypeId,

    [Required]
    RequestPriority Priority,

    DateTime? DueDate,

    [StringLength(RequestConsts.MaxJustificationLength)]
    string? Justification);
