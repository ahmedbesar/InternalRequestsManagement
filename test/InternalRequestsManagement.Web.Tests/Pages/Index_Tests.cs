using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace InternalRequestsManagement.Pages;

[Collection(InternalRequestsManagementTestConsts.CollectionDefinitionName)]
public class Index_Tests : InternalRequestsManagementWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
