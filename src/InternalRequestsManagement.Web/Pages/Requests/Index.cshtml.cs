using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalRequestsManagement.Permissions;
using InternalRequestsManagement.Requests;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.Authorization.Permissions;

namespace InternalRequestsManagement.Web.Pages.Requests;

public class IndexModel : PageModel
{
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanChangeStatus { get; set; }
    public bool CanAssign { get; set; }

    public List<RequestTypeDto> RequestTypes { get; set; } = new();

    private readonly IRequestTypeAppService _requestTypeAppService;
    private readonly IPermissionChecker _permissionChecker;

    public IndexModel(IRequestTypeAppService requestTypeAppService, IPermissionChecker permissionChecker)
    {
        _requestTypeAppService = requestTypeAppService;
        _permissionChecker = permissionChecker;
    }

    public async Task OnGetAsync()
    {
        CanCreate = await _permissionChecker.IsGrantedAsync(InternalRequestsManagementPermissions.Requests.Create);
        CanEdit = await _permissionChecker.IsGrantedAsync(InternalRequestsManagementPermissions.Requests.Edit);
        CanDelete = await _permissionChecker.IsGrantedAsync(InternalRequestsManagementPermissions.Requests.Delete);
        CanChangeStatus = await _permissionChecker.IsGrantedAsync(InternalRequestsManagementPermissions.Requests.ChangeStatus);
        CanAssign = await _permissionChecker.IsGrantedAsync(InternalRequestsManagementPermissions.Requests.Assign);

        var types = await _requestTypeAppService.GetListAsync(isActive: true);
        RequestTypes = types.Items.ToList();
    }
}
