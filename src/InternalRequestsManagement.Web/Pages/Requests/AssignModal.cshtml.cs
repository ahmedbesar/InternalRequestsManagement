using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.OrganizationUnits;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class AssignModalModel : PageModel
{
    public RequestDetailDto? Request { get; set; }
    public List<UserLookupDto> AssignableUsers { get; set; } = new();

    private readonly IRequestAppService _requestAppService;
    private readonly IOrganizationUnitLookupAppService _organizationUnitLookupAppService;

    public AssignModalModel(
        IRequestAppService requestAppService,
        IOrganizationUnitLookupAppService organizationUnitLookupAppService)
    {
        _requestAppService = requestAppService;
        _organizationUnitLookupAppService = organizationUnitLookupAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Request = await _requestAppService.GetAsync(id);
        var users = await _organizationUnitLookupAppService.GetOUAssignableUsersAsync(Request.OrganizationUnitId);
        AssignableUsers = users.Items.ToList();
    }
}
