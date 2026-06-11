using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

public interface IRequestTypeAppService : IApplicationService
{
    /// <summary>Lists request types, optionally filtered by search text, owning OU and active state (management view).</summary>
    Task<ListResultDto<RequestTypeDto>> GetListAsync(
        string? search = null,
        Guid? organizationUnitId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the active request types selectable for the given OU: that OU's own types plus the global ones. Used to populate the request form.</summary>
    Task<ListResultDto<RequestTypeDto>> GetAvailableTypesAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default);
}
