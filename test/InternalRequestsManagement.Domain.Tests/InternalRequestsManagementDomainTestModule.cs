using Volo.Abp.Modularity;

namespace InternalRequestsManagement;

[DependsOn(
    typeof(InternalRequestsManagementDomainModule),
    typeof(InternalRequestsManagementTestBaseModule)
)]
public class InternalRequestsManagementDomainTestModule : AbpModule
{

}
