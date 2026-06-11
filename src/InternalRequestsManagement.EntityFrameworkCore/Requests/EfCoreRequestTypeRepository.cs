using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalRequestsManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace InternalRequestsManagement.Requests;

public class EfCoreRequestTypeRepository :
    EfCoreRepository<InternalRequestsManagementDbContext, RequestType, Guid>,
    IRequestTypeRepository
{
    public EfCoreRequestTypeRepository(IDbContextProvider<InternalRequestsManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<RequestType>> GetListAsync(
        string? search,
        Guid? organizationUnitId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Name.Contains(search));
        }

        if (organizationUnitId.HasValue)
        {
            query = query.Where(t => t.OrganizationUnitId == organizationUnitId.Value
                                     || t.OrganizationUnitId == null);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        return await query.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<List<RequestType>> GetAvailableForOrganizationUnitAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(t => t.IsActive && (t.OrganizationUnitId == null || t.OrganizationUnitId == organizationUnitId))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }
}
