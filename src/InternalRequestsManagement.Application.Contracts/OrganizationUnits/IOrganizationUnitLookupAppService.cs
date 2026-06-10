using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.OrganizationUnits;

public interface IOrganizationUnitLookupAppService : IApplicationService
{
    Task<ListResultDto<OrganizationUnitLookupDto>> GetChildrenAsync(Guid? parentId);

    Task<ListResultDto<OrganizationUnitLookupDto>> GetUserOrganizationUnitPathAsync(Guid userId);
}
