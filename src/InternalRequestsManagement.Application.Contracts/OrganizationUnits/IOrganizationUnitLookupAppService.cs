using System;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.OrganizationUnits;

public interface IOrganizationUnitLookupAppService : IApplicationService
{
    /// <summary>Returns the direct child OUs of the given parent (root OUs when parentId is null). Drives one level of the cascading OU dropdown.</summary>
    Task<ListResultDto<OrganizationUnitLookupDto>> GetChildrenAsync(Guid? parentId);

    /// <summary>Returns the OU path (root to leaf) of a user's organization unit, used to pre-select the cascade for that user.</summary>
    Task<ListResultDto<OrganizationUnitLookupDto>> GetUserOrganizationUnitPathAsync(Guid userId);

    /// <summary>Returns the OU path (root to leaf) for a given OU id, used to pre-fill the cascade when editing.</summary>
    Task<ListResultDto<OrganizationUnitLookupDto>> GetPathAsync(Guid ouId);

    /// <summary>Returns the users that may be assigned to a request in the given OU: members of that OU and all of its descendant OUs.</summary>
    Task<ListResultDto<UserLookupDto>> GetOUAssignableUsersAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default);
}
