using System;
using Volo.Abp;
using System.Threading.Tasks;
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
    private readonly IOrganizationUnitRepository _organizationUnitRepository;

    public CustomIdentityUserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IOptions<IdentityOptions> identityOptions,
        IPermissionChecker permissionChecker,
        IOrganizationUnitRepository organizationUnitRepository)
        : base(userManager, userRepository, roleRepository, identityOptions, permissionChecker)
    {
        _organizationUnitRepository = organizationUnitRepository;
    }

    [Authorize(IdentityPermissions.Users.Create)]
    public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        var organizationUnitId = input.GetProperty<Guid>(IdentityUserExtensionPropertyNames.OrganizationUnitId);
        if (organizationUnitId == Guid.Empty)
        {
            throw new BusinessException(InternalRequestsManagementDomainErrorCodes.OrganizationUnitRequired);
        }

        await _organizationUnitRepository.GetAsync(organizationUnitId);

        var userDto = await base.CreateAsync(input);

        await UserManager.AddToOrganizationUnitAsync(userDto.Id, organizationUnitId);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return userDto;
    }
}
