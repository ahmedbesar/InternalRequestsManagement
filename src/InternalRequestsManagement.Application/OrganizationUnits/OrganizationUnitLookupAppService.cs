using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

[Authorize(IdentityPermissions.Users.Create)]
public class OrganizationUnitLookupAppService : ApplicationService, IOrganizationUnitLookupAppService
{
    private readonly OrganizationUnitManager _organizationUnitManager;

    public OrganizationUnitLookupAppService(OrganizationUnitManager organizationUnitManager)
    {
        _organizationUnitManager = organizationUnitManager;
    }

    public async Task<ListResultDto<OrganizationUnitLookupDto>> GetChildrenAsync(Guid? parentId)
    {
        var children = await _organizationUnitManager.FindChildrenAsync(parentId);

        var items = new OrganizationUnitLookupDto[children.Count];
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var grandChildren = await _organizationUnitManager.FindChildrenAsync(child.Id);
            items[i] = new OrganizationUnitLookupDto(
                child.Id,
                child.DisplayName,
                grandChildren.Count > 0);
        }

        return new ListResultDto<OrganizationUnitLookupDto>(items);
    }
}
