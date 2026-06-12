using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests;

public interface IRequestTypeManager
{
    Task<List<RequestType>> GetListAsync(
        string? search = null,
        Guid? organizationUnitId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<List<RequestType>> GetAvailableForOrganizationUnitAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, OrganizationUnit>> GetOrganizationUnitLookupAsync(
        IEnumerable<RequestType> types,
        CancellationToken cancellationToken = default);
}
