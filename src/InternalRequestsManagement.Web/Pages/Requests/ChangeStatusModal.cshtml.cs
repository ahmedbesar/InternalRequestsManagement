using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class ChangeStatusModalModel : PageModel
{
    public RequestDetailDto? Request { get; set; }
    public List<RequestStatus> AllowedNextStatuses { get; set; } = new();

    private readonly IRequestAppService _requestAppService;

    public ChangeStatusModalModel(IRequestAppService requestAppService)
    {
        _requestAppService = requestAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Request = await _requestAppService.GetAsync(id);
        AllowedNextStatuses = Request.AllowedNextStatuses.ToList();
    }
}
