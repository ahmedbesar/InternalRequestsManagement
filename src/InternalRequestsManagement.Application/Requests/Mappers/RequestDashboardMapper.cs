using System.Collections.Generic;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests.Mappers;

public static class RequestDashboardMapper
{
    public static StatusCountItemDto ToStatusCountItem(StatusCountResult result)
    {
        return new StatusCountItemDto(result.Status, result.Status.ToString(), result.Count);
    }

    public static TypeCountItemDto ToTypeCountItem(TypeCountResult result)
    {
        return new TypeCountItemDto(result.RequestTypeId, result.RequestTypeName, result.Count);
    }

    public static AssigneeCountItemDto ToAssigneeCountItem(AssigneeCountResult result)
    {
        return new AssigneeCountItemDto(result.UserId, result.UserName, result.Count);
    }

    public static OuCountItemDto ToOuCountItem(OuCountResult result, string organizationUnitName)
    {
        return new OuCountItemDto(result.OrganizationUnitId, organizationUnitName, result.Count);
    }

    public static RequestDashboardDto ToDto(
        int openCount,
        int overdueCount,
        int unassignedCount,
        IReadOnlyList<StatusCountItemDto> byStatus,
        IReadOnlyList<TypeCountItemDto> byType,
        IReadOnlyList<OuCountItemDto> byOrganizationUnit,
        IReadOnlyList<AssigneeCountItemDto> topAssignees)
    {
        return new RequestDashboardDto(
            openCount,
            overdueCount,
            unassignedCount,
            byStatus,
            byType,
            byOrganizationUnit,
            topAssignees);
    }
}
