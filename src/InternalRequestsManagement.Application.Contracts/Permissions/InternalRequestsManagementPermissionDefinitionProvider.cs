using InternalRequestsManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace InternalRequestsManagement.Permissions;

public class InternalRequestsManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(InternalRequestsManagementPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(InternalRequestsManagementPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<InternalRequestsManagementResource>(name);
    }
}
