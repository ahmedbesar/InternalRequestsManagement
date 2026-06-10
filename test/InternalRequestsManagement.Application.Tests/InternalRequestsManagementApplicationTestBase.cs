using Volo.Abp.Modularity;

namespace InternalRequestsManagement;

public abstract class InternalRequestsManagementApplicationTestBase<TStartupModule> : InternalRequestsManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
