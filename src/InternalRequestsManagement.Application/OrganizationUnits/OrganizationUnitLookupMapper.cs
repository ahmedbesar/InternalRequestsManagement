using System.Collections.Generic;
using System.Linq;
using InternalRequestsManagement.Requests;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

public static class OrganizationUnitLookupMapper
{
    public static OrganizationUnitLookupDto ToDto(OrganizationUnit organizationUnit, bool hasChildren)
    {
        return new OrganizationUnitLookupDto(
            organizationUnit.Id,
            organizationUnit.DisplayName,
            hasChildren);
    }

    public static UserLookupDto ToDto(IdentityUser user)
    {
        return new UserLookupDto(user.Id, user.UserName, user.Name, user.Surname);
    }

    public static List<UserLookupDto> ToDtos(IEnumerable<IdentityUser> users)
    {
        return users
            .DistinctBy(u => u.Id)
            .OrderBy(u => u.UserName)
            .Select(ToDto)
            .ToList();
    }
}
