using Volo.Abp.Settings;

namespace InternalRequestsManagement.Settings;

public class InternalRequestsManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(InternalRequestsManagementSettings.MySetting1));
    }
}
