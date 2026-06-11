using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class AssignModalModel : PageModel
{
    public RequestDetailDto? Request { get; set; }
    public List<UserLookupDto> AssignableUsers { get; set; } = new();

    private readonly IRequestAppService _requestAppService;

    public AssignModalModel(IRequestAppService requestAppService)
    {
        _requestAppService = requestAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Request = await _requestAppService.GetAsync(id);
        var users = await _requestAppService.GetAssignableUsersAsync(Request.OrganizationUnitId);
        AssignableUsers = users.Items.ToList();
    }
}
