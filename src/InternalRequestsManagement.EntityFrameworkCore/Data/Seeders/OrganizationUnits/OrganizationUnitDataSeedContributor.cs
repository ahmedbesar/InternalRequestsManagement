using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace InternalRequestsManagement.Data.Seeders.OrganizationUnits;

public class OrganizationUnitDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;

    public OrganizationUnitDataSeedContributor(
        OrganizationUnitManager organizationUnitManager,
        IGuidGenerator guidGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant)
    {
        _organizationUnitManager = organizationUnitManager;
        _guidGenerator = guidGenerator;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context.TenantId))
        {
            if (await RootExistsAsync())
            {
                return;
            }

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                var t4Trust = await CreateAsync(OrganizationUnitSeedConsts.T4Trust);
                await uow.SaveChangesAsync();

                var softwareDev = await CreateAsync(OrganizationUnitSeedConsts.SoftwareDevelopment, t4Trust.Id);
                var humanResources = await CreateAsync(OrganizationUnitSeedConsts.HumanResources, t4Trust.Id);
                await uow.SaveChangesAsync();

                await CreateAsync(OrganizationUnitSeedConsts.UiUxTeam, softwareDev.Id);
                await CreateAsync(OrganizationUnitSeedConsts.BackendTeam, softwareDev.Id);
                await CreateAsync(OrganizationUnitSeedConsts.DevOpsTeam, softwareDev.Id);

                await CreateAsync(OrganizationUnitSeedConsts.RecruitmentTeam, humanResources.Id);
                await CreateAsync(OrganizationUnitSeedConsts.EmployeeRelationsTeam, humanResources.Id);
                await CreateAsync(OrganizationUnitSeedConsts.TrainingTeam, humanResources.Id);

                await uow.CompleteAsync();
            }
        }
    }

    private async Task<bool> RootExistsAsync()
    {
        var rootUnits = await _organizationUnitManager.FindChildrenAsync(null);
        return rootUnits.Any(ou => ou.DisplayName == OrganizationUnitSeedConsts.T4Trust);
    }

    private async Task<OrganizationUnit> CreateAsync(string displayName, Guid? parentId = null)
    {
        var organizationUnit = new OrganizationUnit(
            _guidGenerator.Create(),
            displayName,
            parentId,
            _currentTenant.Id);

        await _organizationUnitManager.CreateAsync(organizationUnit);

        // Persist immediately so the next sibling's Code is computed against the
        // already-saved sibling. Without this, ABP assigns identical codes to siblings.
        await _unitOfWorkManager.Current!.SaveChangesAsync();
        return organizationUnit;
    }
}
