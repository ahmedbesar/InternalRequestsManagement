using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

public interface IRequestAppService : IApplicationService
{
    /// <summary>Returns a paged, searchable and filterable list of requests visible to the current user (scoped by OU and the selected list scope).</summary>
    Task<PagedResultDto<RequestDto>> GetListAsync(
        GetRequestListInput input,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a single request with its full detail, including the status history and the allowed next statuses.</summary>
    Task<RequestDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new request for the current user after the domain rules (OU, justification, due date) pass.</summary>
    Task<Result<RequestDto>> CreateAsync(
        CreateRequestDto input,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an editable (non-terminal) request's details, re-validating the dynamic business rules.</summary>
    Task<Result<RequestDto>> UpdateAsync(
        Guid id,
        UpdateRequestDto input,
        CancellationToken cancellationToken = default);

    /// <summary>Moves a request to a new status, enforcing the allowed transitions and the mandatory-note rule.</summary>
    Task<Result<RequestDto>> ChangeStatusAsync(
        Guid id,
        ChangeRequestStatusDto input,
        CancellationToken cancellationToken = default);

    /// <summary>Assigns the request to a user (or unassigns it), ensuring the assignee belongs to the request's OU subtree.</summary>
    Task<Result<RequestDto>> AssignAsync(
        Guid id,
        AssignRequestDto input,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a request.</summary>
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
