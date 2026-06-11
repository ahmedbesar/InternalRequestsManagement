using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

public interface IRequestDashboardAppService : IApplicationService
{
    Task<RequestDashboardDto> GetAsync(
        Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default);
}
