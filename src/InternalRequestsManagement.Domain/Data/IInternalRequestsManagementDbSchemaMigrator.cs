using System.Threading.Tasks;

namespace InternalRequestsManagement.Data;

public interface IInternalRequestsManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
