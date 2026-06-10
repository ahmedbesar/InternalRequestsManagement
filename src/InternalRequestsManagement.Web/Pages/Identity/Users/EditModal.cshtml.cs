using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using InternalRequestsManagement.Identity;
using InternalRequestsManagement.OrganizationUnits;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Validation;

namespace Volo.Abp.Identity.Web.Pages.Identity.Users;

public class EditModalModel : IdentityPageModel
{
    [BindProperty]
    public UserInfoViewModel UserInfo { get; set; } = null!;

    [BindProperty]
    public AssignedRoleViewModel[] Roles { get; set; } = null!;

    public DetailViewModel Detail { get; set; } = null!;

    public List<OrganizationUnitLookupDto> RootOrganizationUnits { get; set; } = new();

    public string InitialOrganizationUnitPathJson { get; set; } = "[]";

    protected IIdentityUserAppService IdentityUserAppService { get; }

    protected IPermissionChecker PermissionChecker { get; }

    public bool IsEditCurrentUser { get; set; }

    private readonly IOrganizationUnitLookupAppService _organizationUnitLookupAppService;

    public EditModalModel(
        IIdentityUserAppService identityUserAppService,
        IPermissionChecker permissionChecker,
        IOrganizationUnitLookupAppService organizationUnitLookupAppService)
    {
        IdentityUserAppService = identityUserAppService;
        PermissionChecker = permissionChecker;
        _organizationUnitLookupAppService = organizationUnitLookupAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync(Guid id)
    {
        var user = await IdentityUserAppService.GetAsync(id);
        UserInfo = ObjectMapper.Map<IdentityUserDto, UserInfoViewModel>(user);

        if (await PermissionChecker.IsGrantedAsync(IdentityPermissions.Users.ManageRoles))
        {
            var assignableRoles = (await IdentityUserAppService.GetAssignableRolesAsync()).Items;
            var currentRoles = (await IdentityUserAppService.GetRolesAsync(id)).Items;

            var combinedRoles = assignableRoles
                .Concat(currentRoles)
                .GroupBy(role => role.Id)
                .Select(group => group.First())
                .ToList();

            Roles = ObjectMapper.Map<IReadOnlyList<IdentityRoleDto>, AssignedRoleViewModel[]>(combinedRoles);

            var currentRoleIds = currentRoles.Select(r => r.Id).ToHashSet();
            var assignableRoleIds = assignableRoles.Select(r => r.Id).ToHashSet();
            foreach (var role in Roles)
            {
                role.IsAssigned = currentRoleIds.Contains(role.Id);
                role.IsAssignable = assignableRoleIds.Contains(role.Id);
            }
        }
        else
        {
            Roles = Array.Empty<AssignedRoleViewModel>();
        }

        IsEditCurrentUser = CurrentUser.Id == id;

        Detail = ObjectMapper.Map<IdentityUserDto, DetailViewModel>(user);
        Detail.CreatedBy = await GetUserNameOrNullAsync(user.CreatorId);
        Detail.ModifiedBy = await GetUserNameOrNullAsync(user.LastModifierId);

        RootOrganizationUnits = (await _organizationUnitLookupAppService.GetChildrenAsync(null)).Items.ToList();

        var organizationUnitPath = (await _organizationUnitLookupAppService.GetUserOrganizationUnitPathAsync(id)).Items.ToList();
        UserInfo.OrganizationUnitId = organizationUnitPath.LastOrDefault()?.Id ?? Guid.Empty;
        InitialOrganizationUnitPathJson = JsonSerializer.Serialize(organizationUnitPath.Select(x => x.Id));

        return Page();
    }

    private async Task<string?> GetUserNameOrNullAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        try
        {
            var user = await IdentityUserAppService.GetAsync(userId.Value);
            return user.UserName;
        }
        catch (EntityNotFoundException)
        {
            return null;
        }
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        ValidateModel();

        var input = ObjectMapper.Map<UserInfoViewModel, IdentityUserUpdateDto>(UserInfo);
        UserInfo.MapExtraPropertiesTo(input);
        input.SetProperty(IdentityUserExtensionPropertyNames.OrganizationUnitId, UserInfo.OrganizationUnitId);
        input.RoleNames = Roles.Where(r => r.IsAssigned).Select(r => r.Name).ToArray();

        await IdentityUserAppService.UpdateAsync(UserInfo.Id, input);

        return NoContent();
    }

    public class UserInfoViewModel : ExtensibleObject, IHasConcurrencyStamp
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [HiddenInput]
        public string ConcurrencyStamp { get; set; } = null!;

        [Required]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
        public string UserName { get; set; } = null!;

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxNameLength))]
        public string? Name { get; set; }

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxSurnameLength))]
        public string? Surname { get; set; }

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
        [DataType(DataType.Password)]
        [DisableAuditing]
        public string? Password { get; set; }

        [Required]
        [EmailAddress]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
        public string Email { get; set; } = null!;

        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPhoneNumberLength))]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public bool LockoutEnabled { get; set; }

        [Required]
        [Display(Name = "OrganizationUnit")]
        public Guid OrganizationUnitId { get; set; }
    }

    public class AssignedRoleViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [HiddenInput]
        public string Name { get; set; } = null!;

        public bool IsAssigned { get; set; }

        public bool IsAssignable { get; set; }
    }

    public class DetailViewModel
    {
        public string? CreatedBy { get; set; }
        public DateTime? CreationTime { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? LastModificationTime { get; set; }

        public DateTimeOffset? LastPasswordChangeTime { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public int AccessFailedCount { get; set; }
    }
}
