using InternalRequestsManagement.Localization;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement;

/* Inherit your application services from this class.
 */
public abstract class InternalRequestsManagementAppService : ApplicationService
{
    protected InternalRequestsManagementAppService()
    {
        LocalizationResource = typeof(InternalRequestsManagementResource);
    }
}
