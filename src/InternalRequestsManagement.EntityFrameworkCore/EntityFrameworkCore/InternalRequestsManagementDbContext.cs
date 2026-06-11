using InternalRequestsManagement.Requests;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace InternalRequestsManagement.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class InternalRequestsManagementDbContext :
    AbpDbContext<InternalRequestsManagementDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestType> RequestTypes { get; set; }


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public InternalRequestsManagementDbContext(DbContextOptions<InternalRequestsManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();
        
        builder.Entity<RequestType>(b =>
        {
            b.ToTable(InternalRequestsManagementConsts.DbTablePrefix + "RequestTypes", InternalRequestsManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(RequestTypeConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(RequestTypeConsts.MaxDescriptionLength);
            b.HasIndex(x => x.OrganizationUnitId);
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<Request>(b =>
        {
            b.ToTable(InternalRequestsManagementConsts.DbTablePrefix + "Requests", InternalRequestsManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(RequestConsts.MaxTitleLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(RequestConsts.MaxDescriptionLength);
            b.Property(x => x.Justification).HasMaxLength(RequestConsts.MaxJustificationLength);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.OrganizationUnitId);
            b.HasIndex(x => x.AssignedUserId);
            b.HasIndex(x => x.DueDate);
            b.HasIndex(x => x.RequesterId);
            b.HasMany(x => x.StatusHistory).WithOne().HasForeignKey(h => h.RequestId).IsRequired();
        });

        builder.Entity<RequestStatusHistory>(b =>
        {
            b.ToTable(InternalRequestsManagementConsts.DbTablePrefix + "RequestStatusHistories", InternalRequestsManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Note).HasMaxLength(RequestConsts.MaxStatusNoteLength);
            b.HasIndex(x => x.RequestId);
        });
    }
}
