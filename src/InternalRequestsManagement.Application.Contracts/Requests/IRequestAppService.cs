using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

public interface IRequestAppService : IApplicationService
{
    Task<PagedResultDto<RequestDto>> GetListAsync(
        GetRequestListInput input,
        CancellationToken cancellationToken = default);

    Task<RequestDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<RequestDto>> CreateAsync(
        CreateRequestDto input,
        CancellationToken cancellationToken = default);

    Task<Result<RequestDto>> UpdateAsync(
        Guid id,
        UpdateRequestDto input,
        CancellationToken cancellationToken = default);

    Task<Result<RequestDto>> ChangeStatusAsync(
        Guid id,
        ChangeRequestStatusDto input,
        CancellationToken cancellationToken = default);

    Task<Result<RequestDto>> AssignAsync(
        Guid id,
        AssignRequestDto input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ListResultDto<RequestTypeDto>> GetAvailableTypesAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default);

    Task<ListResultDto<UserLookupDto>> GetAssignableUsersAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default);
}
