using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InternalRequestsManagement.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class InternalRequestsManagementDbContextFactory : IDesignTimeDbContextFactory<InternalRequestsManagementDbContext>
{
    public InternalRequestsManagementDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        InternalRequestsManagementEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<InternalRequestsManagementDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new InternalRequestsManagementDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../InternalRequestsManagement.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
