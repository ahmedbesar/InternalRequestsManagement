using System;
using Volo.Abp;
using System.Threading.Tasks;
using InternalRequestsManagement.OrganizationUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Identity;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IIdentityUserAppService), typeof(IdentityUserAppService))]
public class CustomIdentityUserAppService : IdentityUserAppService
{
    private readonly OrganizationUnitHierarchyManager _ouHierarchyManager;

    public CustomIdentityUserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IOptions<IdentityOptions> identityOptions,
        IPermissionChecker permissionChecker,
        OrganizationUnitHierarchyManager ouHierarchyManager)
        : base(userManager, userRepository, roleRepository, identityOptions, permissionChecker)
    {
        _ouHierarchyManager = ouHierarchyManager;
    }

    [Authorize(IdentityPermissions.Users.Create)]
    public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        var organizationUnitId = input.GetProperty<Guid>(IdentityUserExtensionPropertyNames.OrganizationUnitId);
        if (organizationUnitId == Guid.Empty)
        {
            throw new BusinessException(InternalRequestsManagementDomainErrorCodes.OrganizationUnitRequired);
        }

        await _ouHierarchyManager.GetAsync(organizationUnitId);

        var userDto = await base.CreateAsync(input);

        await UserManager.AddToOrganizationUnitAsync(userDto.Id, organizationUnitId);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return userDto;
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public override async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        var organizationUnitId = input.GetProperty<Guid>(IdentityUserExtensionPropertyNames.OrganizationUnitId);
        if (organizationUnitId == Guid.Empty)
        {
            throw new BusinessException(InternalRequestsManagementDomainErrorCodes.OrganizationUnitRequired);
        }

        await _ouHierarchyManager.GetAsync(organizationUnitId);

        var userDto = await base.UpdateAsync(id, input);

        await UserManager.SetOrganizationUnitsAsync(id, organizationUnitId);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return userDto;
    }
}
