using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InternalRequestsManagement.Data;
using Volo.Abp.DependencyInjection;

namespace InternalRequestsManagement.EntityFrameworkCore;

public class EntityFrameworkCoreInternalRequestsManagementDbSchemaMigrator
    : IInternalRequestsManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreInternalRequestsManagementDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the InternalRequestsManagementDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<InternalRequestsManagementDbContext>()
            .Database
            .MigrateAsync();
    }
}
