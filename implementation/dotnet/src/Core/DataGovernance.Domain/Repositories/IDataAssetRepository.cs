using DataGovernance.Domain.Entities;

namespace DataGovernance.Domain.Repositories;

/// <summary>
/// Repository interface for DataAsset entity
/// </summary>
public interface IDataAssetRepository : IRepository<DataAsset>
{
    /// <summary>
    /// Search data assets by name or description
    /// </summary>
    Task<IEnumerable<DataAsset>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data assets by tenant
    /// </summary>
    Task<IEnumerable<DataAsset>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data assets by zone
    /// </summary>
    Task<IEnumerable<DataAsset>> GetByZoneAsync(DataZone zone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data assets by classification
    /// </summary>
    Task<IEnumerable<DataAsset>> GetByClassificationAsync(DataClassification classification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data assets by tags
    /// </summary>
    Task<IEnumerable<DataAsset>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data assets by platform
    /// </summary>
    Task<IEnumerable<DataAsset>> GetByPlatformAsync(string platform, CancellationToken cancellationToken = default);
}
