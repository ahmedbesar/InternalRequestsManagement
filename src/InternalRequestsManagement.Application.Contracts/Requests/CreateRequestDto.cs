using System;
using System.ComponentModel.DataAnnotations;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests;

public sealed record CreateRequestDto(
    [Required][StringLength(RequestConsts.MaxTitleLength, MinimumLength = RequestConsts.MinTitleLength)]
    string Title,

    [Required][StringLength(RequestConsts.MaxDescriptionLength)]
    string Description,

    [Required]
    Guid RequestTypeId,

    [Required]
    RequestPriority Priority,

    [Required]
    Guid OrganizationUnitId,

    DateTime? DueDate,

    [StringLength(RequestConsts.MaxJustificationLength)]
    string? Justification);
