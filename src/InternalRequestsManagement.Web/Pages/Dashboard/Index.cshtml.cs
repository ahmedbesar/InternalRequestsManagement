using InternalRequestsManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalRequestsManagement.Web.Pages.Dashboard;

[Authorize(InternalRequestsManagementPermissions.Dashboard.Default)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
