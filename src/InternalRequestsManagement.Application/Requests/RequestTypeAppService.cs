using System;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.Permissions;
using InternalRequestsManagement.Requests.Mappers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InternalRequestsManagement.Requests;

[Authorize(InternalRequestsManagementPermissions.Requests.Default)]
public class RequestTypeAppService : ApplicationService, IRequestTypeAppService
{
    private readonly RequestTypeManager _requestTypeManager;

    public RequestTypeAppService(RequestTypeManager requestTypeManager)
    {
        _requestTypeManager = requestTypeManager;
    }

    public async Task<ListResultDto<RequestTypeDto>> GetListAsync(
        string? search = null,
        Guid? organizationUnitId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var types = await _requestTypeManager.GetListAsync(search, organizationUnitId, isActive, cancellationToken);
        var ouLookup = await _requestTypeManager.GetOrganizationUnitLookupAsync(types, cancellationToken);
        return new ListResultDto<RequestTypeDto>(RequestTypeMapper.ToDtos(types, ouLookup));
    }

    public async Task<ListResultDto<RequestTypeDto>> GetAvailableTypesAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var types = await _requestTypeManager.GetAvailableForOrganizationUnitAsync(organizationUnitId, cancellationToken);
        var ouLookup = await _requestTypeManager.GetOrganizationUnitLookupAsync(types, cancellationToken);
        return new ListResultDto<RequestTypeDto>(RequestTypeMapper.ToDtos(types, ouLookup));
    }
}
