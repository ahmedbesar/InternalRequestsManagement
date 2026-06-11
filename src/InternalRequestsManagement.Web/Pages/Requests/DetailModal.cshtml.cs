using System;
using System.Threading.Tasks;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class DetailModalModel : PageModel
{
    public RequestDetailDto? Request { get; set; }

    private readonly IRequestAppService _requestAppService;

    public DetailModalModel(IRequestAppService requestAppService)
    {
        _requestAppService = requestAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Request = await _requestAppService.GetAsync(id);
    }
}
