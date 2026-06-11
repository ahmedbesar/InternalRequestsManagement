using System;

namespace InternalRequestsManagement.Requests;

public sealed record UserLookupDto(Guid Id, string UserName, string? Name, string? Surname)
{
    public string DisplayName => string.IsNullOrWhiteSpace($"{Name} {Surname}".Trim())
        ? UserName
        : $"{Name} {Surname}".Trim();
}
