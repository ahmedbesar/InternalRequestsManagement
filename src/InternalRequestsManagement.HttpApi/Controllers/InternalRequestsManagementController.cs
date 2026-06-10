using InternalRequestsManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace InternalRequestsManagement.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class InternalRequestsManagementController : AbpControllerBase
{
    protected InternalRequestsManagementController()
    {
        LocalizationResource = typeof(InternalRequestsManagementResource);
    }
}
