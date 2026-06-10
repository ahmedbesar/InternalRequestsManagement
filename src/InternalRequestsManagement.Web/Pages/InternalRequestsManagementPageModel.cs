using InternalRequestsManagement.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace InternalRequestsManagement.Web.Pages;

public abstract class InternalRequestsManagementPageModel : AbpPageModel
{
    protected InternalRequestsManagementPageModel()
    {
        LocalizationResourceType = typeof(InternalRequestsManagementResource);
    }
}
