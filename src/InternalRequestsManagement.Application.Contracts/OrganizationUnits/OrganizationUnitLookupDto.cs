using System;

namespace InternalRequestsManagement.OrganizationUnits;

public sealed record OrganizationUnitLookupDto(Guid Id, string DisplayName, bool HasChildren);
