using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

[Authorize]
public class OrganizationUnitLookupAppService : ApplicationService, IOrganizationUnitLookupAppService
{
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly OrganizationUnitHierarchyManager _ouHierarchyManager;

    public OrganizationUnitLookupAppService(
        OrganizationUnitManager organizationUnitManager,
        IdentityUserManager identityUserManager,
        OrganizationUnitHierarchyManager ouHierarchyManager)
    {
        _organizationUnitManager = organizationUnitManager;
        _identityUserManager = identityUserManager;
        _ouHierarchyManager = ouHierarchyManager;
    }

    public async Task<ListResultDto<OrganizationUnitLookupDto>> GetChildrenAsync(Guid? parentId)
    {
        var children = await _organizationUnitManager.FindChildrenAsync(parentId);
        return new ListResultDto<OrganizationUnitLookupDto>(await MapWithHasChildrenAsync(children));
    }

    [Authorize(IdentityPermissions.Users.Default)]
    public async Task<ListResultDto<OrganizationUnitLookupDto>> GetUserOrganizationUnitPathAsync(Guid userId)
    {
        var user = await _identityUserManager.GetByIdAsync(userId);
        var organizationUnits = await _identityUserManager.GetOrganizationUnitsAsync(user);
        var organizationUnit = organizationUnits.FirstOrDefault();

        if (organizationUnit == null)
            return new ListResultDto<OrganizationUnitLookupDto>(new List<OrganizationUnitLookupDto>());

        return await BuildPathAsync(organizationUnit.Id);
    }

    public Task<ListResultDto<OrganizationUnitLookupDto>> GetPathAsync(Guid ouId)
        => BuildPathAsync(ouId);

    public async Task<ListResultDto<UserLookupDto>> GetOUAssignableUsersAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var ou = await _ouHierarchyManager.GetAsync(organizationUnitId, cancellationToken);

        // ABP's GetUsersInOrganizationUnitAsync with includeChildren:true replaces the
        // manual code-prefix loop that was here before.
        var users = await _identityUserManager.GetUsersInOrganizationUnitAsync(ou, includeChildren: true);

        return new ListResultDto<UserLookupDto>(OrganizationUnitLookupMapper.ToDtos(users));
    }

    private async Task<ListResultDto<OrganizationUnitLookupDto>> BuildPathAsync(Guid organizationUnitId)
    {
        var path = await _ouHierarchyManager.GetPathAsync(organizationUnitId);

        var items = new List<OrganizationUnitLookupDto>();
        foreach (var ou in path)
        {
            var children = await _organizationUnitManager.FindChildrenAsync(ou.Id);
            items.Add(OrganizationUnitLookupMapper.ToDto(ou, children.Count > 0));
        }

        return new ListResultDto<OrganizationUnitLookupDto>(items);
    }

    private async Task<List<OrganizationUnitLookupDto>> MapWithHasChildrenAsync(List<OrganizationUnit> children)
    {
        var items = new OrganizationUnitLookupDto[children.Count];
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var grandChildren = await _organizationUnitManager.FindChildrenAsync(child.Id);
            items[i] = OrganizationUnitLookupMapper.ToDto(child, grandChildren.Count > 0);
        }

        return items.ToList();
    }
}
