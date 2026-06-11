using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

public interface IRequestTypeAppService : IApplicationService
{
    Task<ListResultDto<RequestTypeDto>> GetListAsync(
        string? search = null,
        Guid? organizationUnitId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}
