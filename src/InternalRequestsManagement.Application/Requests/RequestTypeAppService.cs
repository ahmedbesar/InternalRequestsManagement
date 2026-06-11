using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests;

[Authorize(InternalRequestsManagementPermissions.Requests.Default)]
public class RequestTypeAppService : ApplicationService, IRequestTypeAppService
{
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;

    public RequestTypeAppService(
        IRequestTypeRepository requestTypeRepository,
        IOrganizationUnitRepository organizationUnitRepository)
    {
        _requestTypeRepository = requestTypeRepository;
        _organizationUnitRepository = organizationUnitRepository;
    }

    public async Task<ListResultDto<RequestTypeDto>> GetListAsync(
        string? search = null,
        Guid? organizationUnitId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var types = await _requestTypeRepository.GetListAsync(search, organizationUnitId, isActive, cancellationToken);
        var ouIds = types.Where(t => t.OrganizationUnitId.HasValue).Select(t => t.OrganizationUnitId!.Value).Distinct().ToList();

        var ous = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        var ouLookup = ous.Where(o => ouIds.Contains(o.Id)).ToDictionary(o => o.Id);

        var dtos = types.Select(t =>
        {
            ouLookup.TryGetValue(t.OrganizationUnitId ?? Guid.Empty, out var ou);
            return new RequestTypeDto(t.Id, t.Name, t.Description, t.OrganizationUnitId, ou?.DisplayName,
                t.RequiresJustification, t.RequiresDueDate, t.IsActive);
        }).ToList();

        return new ListResultDto<RequestTypeDto>(dtos);
    }

    public async Task<ListResultDto<RequestTypeDto>> GetAvailableTypesAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var types = await _requestTypeRepository.GetAvailableForOrganizationUnitAsync(organizationUnitId, cancellationToken);

        var dtos = new List<RequestTypeDto>();
        foreach (var type in types)
        {
            string? ouName = null;
            if (type.OrganizationUnitId.HasValue)
            {
                var ou = await _organizationUnitRepository.FindAsync(type.OrganizationUnitId.Value, cancellationToken: cancellationToken);
                ouName = ou?.DisplayName;
            }

            dtos.Add(new RequestTypeDto(type.Id, type.Name, type.Description, type.OrganizationUnitId, ouName,
                type.RequiresJustification, type.RequiresDueDate, type.IsActive));
        }

        return new ListResultDto<RequestTypeDto>(dtos);
    }
}
