using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.OrganizationUnits;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class EditModalModel : PageModel
{
    public RequestDetailDto? Request { get; set; }
    public List<OrganizationUnitLookupDto> RootOrganizationUnits { get; set; } = new();
    public List<Guid> OrganizationUnitPath { get; set; } = new();

    private readonly IRequestAppService _requestAppService;
    private readonly IOrganizationUnitLookupAppService _ouLookupAppService;

    public EditModalModel(
        IRequestAppService requestAppService,
        IOrganizationUnitLookupAppService ouLookupAppService)
    {
        _requestAppService = requestAppService;
        _ouLookupAppService = ouLookupAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Request = await _requestAppService.GetAsync(id);

        RootOrganizationUnits = (await _ouLookupAppService.GetChildrenAsync(null)).Items.ToList();

        if (Request.OrganizationUnitId != Guid.Empty)
        {
            OrganizationUnitPath = (await _ouLookupAppService.GetPathAsync(Request.OrganizationUnitId))
                .Items.Select(o => o.Id).ToList();
        }
    }
}
