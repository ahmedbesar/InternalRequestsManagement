using InternalRequestsManagement.Samples;
using Xunit;

namespace InternalRequestsManagement.EntityFrameworkCore.Domains;

[Collection(InternalRequestsManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<InternalRequestsManagementEntityFrameworkCoreTestModule>
{

}
