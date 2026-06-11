using System.Threading.Tasks;
using InternalRequestsManagement.Localization;
using InternalRequestsManagement.Permissions;
using InternalRequestsManagement.MultiTenancy;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;

namespace InternalRequestsManagement.Web.Menus;

public class InternalRequestsManagementMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<InternalRequestsManagementResource>();

        context.Menu.AddItem(
            new ApplicationMenuItem(
                InternalRequestsManagementMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        if (await context.IsGrantedAsync(InternalRequestsManagementPermissions.Dashboard.Default))
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    InternalRequestsManagementMenus.Dashboard,
                    l["Menu:Dashboard"],
                    "/Dashboard",
                    icon: "fa fa-tachometer-alt",
                    order: 2
                )
            );
        }

        if (await context.IsGrantedAsync(InternalRequestsManagementPermissions.Requests.Default))
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    InternalRequestsManagementMenus.Requests,
                    l["Menu:Requests"],
                    "/Requests",
                    icon: "fa fa-tasks",
                    order: 3
                )
            );
        }

        var administration = context.Menu.GetAdministration();
        administration.Order = 6;

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 8);
    }
}
