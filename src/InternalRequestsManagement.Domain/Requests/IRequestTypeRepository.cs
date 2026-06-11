using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace InternalRequestsManagement.Requests;

public interface IRequestTypeRepository : IRepository<RequestType, Guid>
{
    Task<List<RequestType>> GetListAsync(
        string? search,
        Guid? organizationUnitId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<List<RequestType>> GetAvailableForOrganizationUnitAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default);
}
