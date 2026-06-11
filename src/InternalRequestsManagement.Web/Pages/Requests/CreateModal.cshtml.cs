using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.OrganizationUnits;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class CreateModalModel : PageModel
{
    public List<OrganizationUnitLookupDto> RootOrganizationUnits { get; set; } = new();

    private readonly IOrganizationUnitLookupAppService _ouLookupAppService;

    public CreateModalModel(IOrganizationUnitLookupAppService ouLookupAppService)
    {
        _ouLookupAppService = ouLookupAppService;
    }

    public async Task OnGetAsync()
    {
        RootOrganizationUnits = (await _ouLookupAppService.GetChildrenAsync(null)).Items.ToList();
    }
}
