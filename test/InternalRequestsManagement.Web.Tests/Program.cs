using Microsoft.AspNetCore.Builder;
using InternalRequestsManagement;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("InternalRequestsManagement.Web.csproj"); 
await builder.RunAbpModuleAsync<InternalRequestsManagementWebTestModule>(applicationName: "InternalRequestsManagement.Web");

public partial class Program
{
}
