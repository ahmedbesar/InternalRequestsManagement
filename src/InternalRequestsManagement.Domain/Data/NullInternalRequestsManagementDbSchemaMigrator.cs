using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace InternalRequestsManagement.Data;

/* This is used if database provider does't define
 * IInternalRequestsManagementDbSchemaMigrator implementation.
 */
public class NullInternalRequestsManagementDbSchemaMigrator : IInternalRequestsManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
