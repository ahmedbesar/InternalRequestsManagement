using InternalRequestsManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace InternalRequestsManagement.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(InternalRequestsManagementEntityFrameworkCoreModule),
    typeof(InternalRequestsManagementApplicationContractsModule)
)]
public class InternalRequestsManagementDbMigratorModule : AbpModule
{
}
