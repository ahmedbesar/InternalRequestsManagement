using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.Identity;
using InternalRequestsManagement.OrganizationUnits;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Validation;

namespace Volo.Abp.Identity.Web.Pages.Identity.Users;

public class CreateModalModel : IdentityPageModel
{
    [BindProperty]
    public UserInfoViewModel UserInfo { get; set; } = null!;

    [BindProperty]
    public AssignedRoleViewModel[] Roles { get; set; } = null!;

    public List<OrganizationUnitLookupDto> RootOrganizationUnits { get; set; } = new();

    protected IIdentityUserAppService IdentityUserAppService { get; }

    private readonly IOrganizationUnitLookupAppService _organizationUnitLookupAppService;

    public CreateModalModel(
        IIdentityUserAppService identityUserAppService,
        IOrganizationUnitLookupAppService organizationUnitLookupAppService)
    {
        IdentityUserAppService = identityUserAppService;
        _organizationUnitLookupAppService = organizationUnitLookupAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        UserInfo = new UserInfoViewModel();

        var roleDtoList = (await IdentityUserAppService.GetAssignableRolesAsync()).Items;

        Roles = roleDtoList.Select(r => new AssignedRoleViewModel
        {
            Name = r.Name,
            IsDefault = r.IsDefault
        }).ToArray();

        foreach (var role in Roles)
        {
            role.IsAssigned = role.IsDefault;
        }

        RootOrganizationUnits = (await _organizationUnitLookupAppService.GetChildrenAsync(null)).Items.ToList();

        return Page();
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        ValidateModel();

        var input = new IdentityUserCreateDto
        {
            UserName = UserInfo.UserName,
            Name = UserInfo.Name,
            Surname = UserInfo.Surname,
            Email = UserInfo.Email,
            PhoneNumber = UserInfo.PhoneNumber,
            IsActive = UserInfo.IsActive,
            LockoutEnabled = UserInfo.LockoutEnabled,
            Password = UserInfo.Password
        };
        UserInfo.MapExtraPropertiesTo(input);
        input.SetProperty(IdentityUserExtensionPropertyNames.OrganizationUnitId, UserInfo.OrganizationUnitId);
        input.RoleNames = Roles.Where(r => r.IsAssigned).Select(r => r.Name).ToArray();

        await IdentityUserAppService.CreateAsync(input);

        return NoContent();
    }

    public class UserInfoViewModel : ExtensibleObject
    {
        [Required]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
        public string UserName { get; set; } = null!;

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxNameLength))]
        public string? Name { get; set; }

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxSurnameLength))]
        public string? Surname { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisableAuditing]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
        public string Password { get; set; } = null!;

        [Required]
        [EmailAddress]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
        public string Email { get; set; } = null!;

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPhoneNumberLength))]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public bool LockoutEnabled { get; set; } = true;

        [Required]
        [Display(Name = "OrganizationUnit")]
        public Guid OrganizationUnitId { get; set; }
    }

    public class AssignedRoleViewModel
    {
        [Required]
        [HiddenInput]
        public string Name { get; set; } = null!;

        public bool IsAssigned { get; set; }

        public bool IsDefault { get; set; }
    }
}
