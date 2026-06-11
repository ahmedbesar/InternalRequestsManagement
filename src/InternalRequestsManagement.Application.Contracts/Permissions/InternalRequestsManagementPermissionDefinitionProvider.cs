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

        myGroup.AddPermission(
            InternalRequestsManagementPermissions.Dashboard.Default,
            L("Permission:Dashboard"));

        var requestsPermission = myGroup.AddPermission(
            InternalRequestsManagementPermissions.Requests.Default,
            L("Permission:Requests"));
        requestsPermission.AddChild(InternalRequestsManagementPermissions.Requests.Create, L("Permission:Requests.Create"));
        requestsPermission.AddChild(InternalRequestsManagementPermissions.Requests.Edit, L("Permission:Requests.Edit"));
        requestsPermission.AddChild(InternalRequestsManagementPermissions.Requests.Delete, L("Permission:Requests.Delete"));
        requestsPermission.AddChild(InternalRequestsManagementPermissions.Requests.ChangeStatus, L("Permission:Requests.ChangeStatus"));
        requestsPermission.AddChild(InternalRequestsManagementPermissions.Requests.Assign, L("Permission:Requests.Assign"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<InternalRequestsManagementResource>(name);
    }
}
