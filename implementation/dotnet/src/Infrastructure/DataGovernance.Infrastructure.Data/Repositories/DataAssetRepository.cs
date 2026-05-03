using DataGovernance.Domain.Entities;
using DataGovernance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataGovernance.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for DataAsset entity
/// </summary>
public class DataAssetRepository : Repository<DataAsset>, IDataAssetRepository
{
    public DataAssetRepository(DataGovernanceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DataAsset>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Name.Contains(searchTerm) || a.Description.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataAsset>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataAsset>> GetByZoneAsync(DataZone zone, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Zone == zone)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataAsset>> GetByClassificationAsync(DataClassification classification, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Classification == classification)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataAsset>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        var tagList = tags.ToList();
        return await _dbSet
            .Where(a => a.Tags.Any(t => tagList.Contains(t)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DataAsset>> GetByPlatformAsync(string platform, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Platform == platform)
            .ToListAsync(cancellationToken);
    }
}
