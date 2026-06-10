using Volo.Abp.Modularity;

namespace InternalRequestsManagement;

/* Inherit from this class for your domain layer tests. */
public abstract class InternalRequestsManagementDomainTestBase<TStartupModule> : InternalRequestsManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
