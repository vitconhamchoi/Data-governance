using DataGovernance.Domain.Entities;
using DataGovernance.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DataGovernance.API.Controllers;

/// <summary>
/// Data Assets management endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class DataAssetsController : ControllerBase
{
    private readonly IDataAssetRepository _repository;
    private readonly ILogger<DataAssetsController> _logger;

    public DataAssetsController(
        IDataAssetRepository repository,
        ILogger<DataAssetsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get all data assets
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataAsset>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all data assets");
        var assets = await _repository.GetAllAsync(cancellationToken);
        return Ok(assets);
    }

    /// <summary>
    /// Get data asset by ID
    /// </summary>
    /// <param name="id">Asset ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataAsset>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting data asset {AssetId}", id);
        var asset = await _repository.GetByIdAsync(id, cancellationToken);

        if (asset == null)
        {
            return NotFound();
        }

        return Ok(asset);
    }

    /// <summary>
    /// Search data assets by name or description
    /// </summary>
    /// <param name="query">Search query</param>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataAsset>>> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching data assets with query: {Query}", query);
        var assets = await _repository.SearchAsync(query, cancellationToken);
        return Ok(assets);
    }

    /// <summary>
    /// Get data assets by tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataAsset>>> GetByTenant(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting data assets for tenant {TenantId}", tenantId);
        var assets = await _repository.GetByTenantAsync(tenantId, cancellationToken);
        return Ok(assets);
    }

    /// <summary>
    /// Create a new data asset
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataAsset>> Create(
        [FromBody] DataAsset asset,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new data asset: {AssetName}", asset.Name);

        asset.Id = Guid.NewGuid();
        asset.CreatedAt = DateTimeOffset.UtcNow;
        asset.UpdatedAt = DateTimeOffset.UtcNow;

        var created = await _repository.AddAsync(asset, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing data asset
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] DataAsset asset,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating data asset {AssetId}", id);

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        asset.Id = id;
        asset.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(asset, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete a data asset
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting data asset {AssetId}", id);

        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
