using Volo.Abp.Modularity;

namespace InternalRequestsManagement;

[DependsOn(
    typeof(InternalRequestsManagementApplicationModule),
    typeof(InternalRequestsManagementDomainTestModule)
)]
public class InternalRequestsManagementApplicationTestModule : AbpModule
{

}
