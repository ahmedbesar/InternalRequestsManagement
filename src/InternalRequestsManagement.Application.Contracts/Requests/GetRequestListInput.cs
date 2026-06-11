using System;
using InternalRequestsManagement.Requests;
using Volo.Abp.Application.Dtos;

namespace InternalRequestsManagement.Requests;

public class GetRequestListInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
    public RequestStatus? Status { get; set; }
    public RequestPriority? Priority { get; set; }
    public Guid? RequestTypeId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public RequestListScope Scope { get; set; } = RequestListScope.All;
}
