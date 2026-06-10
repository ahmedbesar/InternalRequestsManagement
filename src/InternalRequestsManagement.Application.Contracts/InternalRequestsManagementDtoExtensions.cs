using System;
using System.ComponentModel.DataAnnotations;
using InternalRequestsManagement.Identity;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace InternalRequestsManagement;

public static class InternalRequestsManagementDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance
                .AddOrUpdateProperty<IdentityUserCreateDto, Guid>(
                    IdentityUserExtensionPropertyNames.OrganizationUnitId,
                    property =>
                    {
                        property.Attributes.Add(new RequiredAttribute());
                    });

            ObjectExtensionManager.Instance
                .AddOrUpdateProperty<IdentityUserUpdateDto, Guid>(
                    IdentityUserExtensionPropertyNames.OrganizationUnitId,
                    property =>
                    {
                        property.Attributes.Add(new RequiredAttribute());
                    });
        });
    }
}
