using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.OrganizationUnits;

/// <summary>
/// Handles all OrganizationUnit / User → DTO conversion.
/// The async overload requires <see cref="OrganizationUnitManager"/> to determine
/// whether each OU has children, so the class is injectable rather than static.
/// </summary>
public class OrganizationUnitLookupMapper : ITransientDependency
{
    private readonly OrganizationUnitManager _organizationUnitManager;

    public OrganizationUnitLookupMapper(OrganizationUnitManager organizationUnitManager)
    {
        _organizationUnitManager = organizationUnitManager;
    }

    /// <summary>
    /// Maps a list of OUs to DTOs, checking each one for children so the UI
    /// knows whether to show an expand arrow.
    /// </summary>
    public async Task<List<OrganizationUnitLookupDto>> MapWithHasChildrenAsync(
        List<OrganizationUnit> organizationUnits)
    {
        var items = new OrganizationUnitLookupDto[organizationUnits.Count];
        for (var i = 0; i < organizationUnits.Count; i++)
        {
            var ou = organizationUnits[i];
            var children = await _organizationUnitManager.FindChildrenAsync(ou.Id);
            items[i] = ToDto(ou, children.Count > 0);
        }

        return items.ToList();
    }

    // ── Pure synchronous mapping ──────────────────────────────────────────────

    public static OrganizationUnitLookupDto ToDto(OrganizationUnit organizationUnit, bool hasChildren)
    {
        return new OrganizationUnitLookupDto(
            organizationUnit.Id,
            organizationUnit.DisplayName,
            hasChildren);
    }

    public static UserLookupDto ToDto(IdentityUser user)
    {
        return new UserLookupDto(user.Id, user.UserName, user.Name, user.Surname);
    }

    public static List<UserLookupDto> ToDtos(IEnumerable<IdentityUser> users)
    {
        return users
            .DistinctBy(u => u.Id)
            .OrderBy(u => u.UserName)
            .Select(ToDto)
            .ToList();
    }
}
