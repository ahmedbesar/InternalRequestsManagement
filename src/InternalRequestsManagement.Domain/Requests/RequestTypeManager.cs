using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace InternalRequestsManagement.Requests;

/// <summary>
/// Domain service that wraps all <see cref="IRequestTypeRepository"/> queries
/// and provides the OU-name lookup needed for DTO mapping.
/// </summary>
public class RequestTypeManager : DomainService
{
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;

    public RequestTypeManager(
        IRequestTypeRepository requestTypeRepository,
        IOrganizationUnitRepository organizationUnitRepository)
    {
        _requestTypeRepository = requestTypeRepository;
        _organizationUnitRepository = organizationUnitRepository;
    }

    /// <summary>Returns request types optionally filtered by search text, OU and active flag.</summary>
    public Task<List<RequestType>> GetListAsync(
        string? search = null,
        Guid? organizationUnitId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
        => _requestTypeRepository.GetListAsync(search, organizationUnitId, isActive, cancellationToken);

    /// <summary>Returns request types available for the given OU (global types + OU-specific types in the same subtree).</summary>
    public Task<List<RequestType>> GetAvailableForOrganizationUnitAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
        => _requestTypeRepository.GetAvailableForOrganizationUnitAsync(organizationUnitId, cancellationToken);

    /// <summary>
    /// Returns a dictionary of OU id → OU for every OU referenced by the given types.
    /// Lets the application layer resolve OU display names for DTO mapping
    /// without touching <see cref="IOrganizationUnitRepository"/> directly.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, OrganizationUnit>> GetOrganizationUnitLookupAsync(
        IEnumerable<RequestType> types,
        CancellationToken cancellationToken = default)
    {
        var ouIds = types
            .Where(t => t.OrganizationUnitId.HasValue)
            .Select(t => t.OrganizationUnitId!.Value)
            .Distinct()
            .ToList();

        if (ouIds.Count == 0)
            return new Dictionary<Guid, OrganizationUnit>();

        var allOus = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        return allOus.Where(o => ouIds.Contains(o.Id)).ToDictionary(o => o.Id);
    }
}
