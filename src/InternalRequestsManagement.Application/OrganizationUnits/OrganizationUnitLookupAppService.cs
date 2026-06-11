using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

[Authorize]
public class OrganizationUnitLookupAppService : ApplicationService, IOrganizationUnitLookupAppService
{
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IIdentityUserRepository _userRepository;

    public OrganizationUnitLookupAppService(
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityUserRepository userRepository)
    {
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _userRepository = userRepository;
    }

    public async Task<ListResultDto<OrganizationUnitLookupDto>> GetChildrenAsync(Guid? parentId)
    {
        var children = await _organizationUnitManager.FindChildrenAsync(parentId);
        return new ListResultDto<OrganizationUnitLookupDto>(await MapWithHasChildrenAsync(children));
    }

    [Authorize(IdentityPermissions.Users.Default)]
    public async Task<ListResultDto<OrganizationUnitLookupDto>> GetUserOrganizationUnitPathAsync(Guid userId)
    {
        var organizationUnits = await _userRepository.GetOrganizationUnitsAsync(userId);
        var organizationUnit = organizationUnits.FirstOrDefault();
        if (organizationUnit == null)
        {
            return new ListResultDto<OrganizationUnitLookupDto>(new List<OrganizationUnitLookupDto>());
        }

        return await BuildPathAsync(organizationUnit.Id);
    }

    public Task<ListResultDto<OrganizationUnitLookupDto>> GetPathAsync(Guid ouId)
        => BuildPathAsync(ouId);

    public async Task<ListResultDto<UserLookupDto>> GetOUAssignableUsersAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var ou = await _organizationUnitRepository.GetAsync(organizationUnitId, cancellationToken: cancellationToken);

        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        var subtreeOuIds = allOus
            .Where(x => x.Code == ou.Code || x.Code.StartsWith(ou.Code + "."))
            .Select(x => x.Id)
            .ToList();

        var users = new List<IdentityUser>();
        foreach (var ouId in subtreeOuIds)
        {
            var ouUsers = await _userRepository.GetListAsync(organizationUnitId: ouId, cancellationToken: cancellationToken);
            users.AddRange(ouUsers);
        }

        var dtos = OrganizationUnitLookupMapper.ToDtos(users);

        return new ListResultDto<UserLookupDto>(dtos);
    }

    private async Task<ListResultDto<OrganizationUnitLookupDto>> BuildPathAsync(Guid organizationUnitId)
    {
        var path = new List<OrganizationUnit>();
        var current = await _organizationUnitRepository.GetAsync(organizationUnitId);

        while (true)
        {
            path.Insert(0, current);
            if (!current.ParentId.HasValue)
            {
                break;
            }

            current = await _organizationUnitRepository.GetAsync(current.ParentId.Value);
        }

        var items = new List<OrganizationUnitLookupDto>();
        foreach (var organizationUnit in path)
        {
            var children = await _organizationUnitManager.FindChildrenAsync(organizationUnit.Id);
            items.Add(OrganizationUnitLookupMapper.ToDto(organizationUnit, children.Count > 0));
        }

        return new ListResultDto<OrganizationUnitLookupDto>(items);
    }

    private async Task<List<OrganizationUnitLookupDto>> MapWithHasChildrenAsync(List<OrganizationUnit> children)
    {
        var items = new OrganizationUnitLookupDto[children.Count];
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var grandChildren = await _organizationUnitManager.FindChildrenAsync(child.Id);
            items[i] = OrganizationUnitLookupMapper.ToDto(child, grandChildren.Count > 0);
        }

        return items.ToList();
    }
}
