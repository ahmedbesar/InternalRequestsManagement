using Riok.Mapperly.Abstractions;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using EditUserModalModel = Volo.Abp.Identity.Web.Pages.Identity.Users.EditModalModel;

namespace InternalRequestsManagement.Web;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IdentityUserDtoToEditUserModalModelUserInfoViewModelMapper
    : MapperBase<IdentityUserDto, EditUserModalModel.UserInfoViewModel>
{
    [MapperIgnoreTarget(nameof(EditUserModalModel.UserInfoViewModel.Password))]
    [MapperIgnoreTarget(nameof(EditUserModalModel.UserInfoViewModel.OrganizationUnitId))]
    public override partial EditUserModalModel.UserInfoViewModel Map(IdentityUserDto source);

    [MapperIgnoreTarget(nameof(EditUserModalModel.UserInfoViewModel.Password))]
    [MapperIgnoreTarget(nameof(EditUserModalModel.UserInfoViewModel.OrganizationUnitId))]
    public override partial void Map(IdentityUserDto source, EditUserModalModel.UserInfoViewModel destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class EditUserModalModelUserInfoViewModelToIdentityUserUpdateDtoMapper
    : MapperBase<EditUserModalModel.UserInfoViewModel, IdentityUserUpdateDto>
{
    [MapperIgnoreTarget(nameof(IdentityUserUpdateDto.RoleNames))]
    [MapperIgnoreSource(nameof(EditUserModalModel.UserInfoViewModel.OrganizationUnitId))]
    public override partial IdentityUserUpdateDto Map(EditUserModalModel.UserInfoViewModel source);

    [MapperIgnoreTarget(nameof(IdentityUserUpdateDto.RoleNames))]
    [MapperIgnoreSource(nameof(EditUserModalModel.UserInfoViewModel.OrganizationUnitId))]
    public override partial void Map(EditUserModalModel.UserInfoViewModel source, IdentityUserUpdateDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IdentityRoleDtoToEditUserModalModelAssignedRoleViewModelMapper
    : MapperBase<IdentityRoleDto, EditUserModalModel.AssignedRoleViewModel>
{
    [MapperIgnoreTarget(nameof(EditUserModalModel.AssignedRoleViewModel.IsAssigned))]
    [MapperIgnoreTarget(nameof(EditUserModalModel.AssignedRoleViewModel.IsAssignable))]
    public override partial EditUserModalModel.AssignedRoleViewModel Map(IdentityRoleDto source);

    [MapperIgnoreTarget(nameof(EditUserModalModel.AssignedRoleViewModel.IsAssigned))]
    [MapperIgnoreTarget(nameof(EditUserModalModel.AssignedRoleViewModel.IsAssignable))]
    public override partial void Map(IdentityRoleDto source, EditUserModalModel.AssignedRoleViewModel destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IdentityUserDtoToEditUserModalModelDetailViewModelMapper
    : MapperBase<IdentityUserDto, EditUserModalModel.DetailViewModel>
{
    [MapperIgnoreTarget(nameof(EditUserModalModel.DetailViewModel.CreatedBy))]
    [MapperIgnoreTarget(nameof(EditUserModalModel.DetailViewModel.ModifiedBy))]
    public override partial EditUserModalModel.DetailViewModel Map(IdentityUserDto source);

    [MapperIgnoreTarget(nameof(EditUserModalModel.DetailViewModel.CreatedBy))]
    [MapperIgnoreTarget(nameof(EditUserModalModel.DetailViewModel.ModifiedBy))]
    public override partial void Map(IdentityUserDto source, EditUserModalModel.DetailViewModel destination);
}
