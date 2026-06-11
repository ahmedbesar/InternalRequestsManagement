using System;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.Data.Seeders.OrganizationUnits;
using InternalRequestsManagement.Requests;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace InternalRequestsManagement.Data.Seeders.Requests;

public class RequestTypeDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;

    public RequestTypeDataSeedContributor(
        IRequestTypeRepository requestTypeRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        IGuidGenerator guidGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant)
    {
        _requestTypeRepository = requestTypeRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _guidGenerator = guidGenerator;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context.TenantId))
        {
            if (await _requestTypeRepository.GetCountAsync() > 0)
            {
                return;
            }

            using var uow = _unitOfWorkManager.Begin(requiresNew: true);

            var allOus = await _organizationUnitRepository.GetListAsync();

            var softwareDev    = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.SoftwareDevelopment);
            var humanResources = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.HumanResources);
            var uiUx           = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.UiUxTeam);
            var backend        = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.BackendTeam);
            var devOps         = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.DevOpsTeam);
            var recruitment    = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.RecruitmentTeam);
            var empRelations   = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.EmployeeRelationsTeam);
            var training       = allOus.FirstOrDefault(ou => ou.DisplayName == OrganizationUnitSeedConsts.TrainingTeam);

            // Stage 2 — Software Development
            await CreateAsync("Bug Report", softwareDev?.Id, context.TenantId, "Report a software bug or defect");
            await CreateAsync("Feature Request", softwareDev?.Id, context.TenantId, "Request a new feature or improvement", requiresJustification: true);
            await CreateAsync("Infrastructure Request", softwareDev?.Id, context.TenantId, "Request infrastructure changes or provisioning", requiresJustification: true, requiresDueDate: true);
            await CreateAsync("Code Review Request", softwareDev?.Id, context.TenantId, "Request a code review for a pull request");

            // Stage 2 — Human Resources
            await CreateAsync("Leave Request", humanResources?.Id, context.TenantId, "Request time off or leave", requiresDueDate: true);
            await CreateAsync("Onboarding Request", humanResources?.Id, context.TenantId, "New employee onboarding setup", requiresDueDate: true);
            await CreateAsync("Training Request", humanResources?.Id, context.TenantId, "Request enrollment in a training or course", requiresJustification: true, requiresDueDate: true);
            await CreateAsync("Policy Exception Request", humanResources?.Id, context.TenantId, "Request an exception to an HR policy", requiresJustification: true);

            // Stage 3 — UI/UX Team
            await CreateAsync("UI Design Review", uiUx?.Id, context.TenantId, "Request a review of a UI or UX design");

            // Stage 3 — Backend Team
            await CreateAsync("API Development Request", backend?.Id, context.TenantId, "Request development of a new API endpoint or service", requiresJustification: true);

            // Stage 3 — DevOps Team
            await CreateAsync("Deployment Request", devOps?.Id, context.TenantId, "Request a deployment to staging or production", requiresJustification: true, requiresDueDate: true);

            // Stage 3 — Recruitment Team
            await CreateAsync("Job Posting Request", recruitment?.Id, context.TenantId, "Request creation of a new job posting", requiresJustification: true);

            // Stage 3 — Employee Relations Team
            await CreateAsync("Grievance Request", empRelations?.Id, context.TenantId, "Submit a formal workplace grievance", requiresJustification: true);

            // Stage 3 — Training Team
            await CreateAsync("Training Material Request", training?.Id, context.TenantId, "Request creation or update of training materials", requiresDueDate: true);

            // Global (available to all OUs)
            await CreateAsync("General Request", null, context.TenantId, "General purpose request");
            await CreateAsync("Access Request", null, context.TenantId, "Request system or resource access", requiresJustification: true);
            await CreateAsync("Document Request", null, context.TenantId, "Request a document or certificate");

            await uow.CompleteAsync();
        }
    }

    private async Task CreateAsync(
        string name,
        Guid? organizationUnitId,
        Guid? tenantId,
        string? description = null,
        bool requiresJustification = false,
        bool requiresDueDate = false)
    {
        var requestType = RequestType.Create(
            _guidGenerator.Create(),
            name,
            tenantId,
            organizationUnitId,
            description,
            requiresJustification,
            requiresDueDate);

        await _requestTypeRepository.InsertAsync(requestType);
    }
}
