using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using InternalRequestsManagement.Localization;

namespace InternalRequestsManagement.Web;

[Dependency(ReplaceServices = true)]
public class InternalRequestsManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<InternalRequestsManagementResource> _localizer;

    public InternalRequestsManagementBrandingProvider(IStringLocalizer<InternalRequestsManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
