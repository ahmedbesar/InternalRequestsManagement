using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

public interface IRequestDashboardAppService : IApplicationService
{
    /// <summary>Builds the dashboard summary (open/overdue/unassigned counts and breakdowns by status, type, OU and top assignees), scoped to the current user's OU context.</summary>
    Task<RequestDashboardDto> GetAsync(
        Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default);
}
