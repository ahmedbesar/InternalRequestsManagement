using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace InternalRequestsManagement.Requests;

public class RequestType : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid? OrganizationUnitId { get; private set; }

    public bool RequiresJustification { get; private set; }

    public bool RequiresDueDate { get; private set; }

    public bool IsActive { get; private set; }

    private RequestType() { }

    public static RequestType Create(
        Guid id,
        string name,
        Guid? tenantId,
        Guid? organizationUnitId = null,
        string? description = null,
        bool requiresJustification = false,
        bool requiresDueDate = false)
    {
        return new RequestType
        {
            Id = id,
            TenantId = tenantId,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), RequestTypeConsts.MaxNameLength, RequestTypeConsts.MinNameLength),
            Description = description,
            OrganizationUnitId = organizationUnitId,
            RequiresJustification = requiresJustification,
            RequiresDueDate = requiresDueDate,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string? description,
        Guid? organizationUnitId,
        bool requiresJustification,
        bool requiresDueDate)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), RequestTypeConsts.MaxNameLength, RequestTypeConsts.MinNameLength);
        Description = description;
        OrganizationUnitId = organizationUnitId;
        RequiresJustification = requiresJustification;
        RequiresDueDate = requiresDueDate;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public bool IsAvailableFor(Guid organizationUnitId) =>
        OrganizationUnitId == null || OrganizationUnitId == organizationUnitId;
}
