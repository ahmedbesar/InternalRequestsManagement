using System;
using System.Collections.Generic;
using System.Linq;
using InternalRequestsManagement.Requests;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests.Mappers;

public static class RequestTypeMapper
{
    public static RequestTypeDto ToDto(RequestType type, string? organizationUnitName)
    {
        return new RequestTypeDto(
            type.Id,
            type.Name,
            type.Description,
            type.OrganizationUnitId,
            organizationUnitName,
            type.RequiresJustification,
            type.RequiresDueDate,
            type.IsActive);
    }

    public static List<RequestTypeDto> ToDtos(
        IEnumerable<RequestType> types,
        IReadOnlyDictionary<Guid, OrganizationUnit> ouLookup)
    {
        return types.Select(t =>
        {
            ouLookup.TryGetValue(t.OrganizationUnitId ?? Guid.Empty, out var ou);
            return ToDto(t, ou?.DisplayName);
        }).ToList();
    }
}
