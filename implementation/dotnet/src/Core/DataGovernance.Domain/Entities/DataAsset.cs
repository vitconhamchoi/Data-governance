namespace DataGovernance.Domain.Entities;

/// <summary>
/// Represents a data asset in the governance catalog
/// </summary>
public class DataAsset : BaseEntity
{
    /// <summary>
    /// Name of the data asset
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Fully qualified name (e.g., database.schema.table)
    /// </summary>
    public string QualifiedName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the data asset
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of data asset (Table, View, Column, File, API, etc.)
    /// </summary>
    public DataAssetType AssetType { get; set; }

    /// <summary>
    /// Platform where the data asset resides (PostgreSQL, S3, Kafka, etc.)
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// URI or connection string to the data asset
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Data classification level (Public, Internal, Confidential, Restricted)
    /// </summary>
    public DataClassification Classification { get; set; }

    /// <summary>
    /// Data zone in lakehouse (Raw, Standardized, Curated, Trusted)
    /// </summary>
    public DataZone Zone { get; set; }

    /// <summary>
    /// Owner of the data asset
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Steward responsible for data quality
    /// </summary>
    public string Steward { get; set; } = string.Empty;

    /// <summary>
    /// Tags associated with the data asset
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Metadata properties
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Quality score (0-100)
    /// </summary>
    public double QualityScore { get; set; }

    /// <summary>
    /// Tenant/organization ID for multi-tenancy
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Parent asset ID (for hierarchical assets)
    /// </summary>
    public Guid? ParentAssetId { get; set; }

    /// <summary>
    /// Schema definition (JSON)
    /// </summary>
    public string? Schema { get; set; }
}

/// <summary>
/// Type of data asset
/// </summary>
public enum DataAssetType
{
    Database,
    Table,
    View,
    Column,
    File,
    Directory,
    Topic,
    Stream,
    API,
    Dashboard,
    Report,
    Model,
    Other
}

/// <summary>
/// Data classification level
/// </summary>
public enum DataClassification
{
    Public,
    Internal,
    Confidential,
    Restricted,
    PII,
    PHI
}

/// <summary>
/// Data zone in multi-zone lakehouse
/// </summary>
public enum DataZone
{
    Raw,
    Standardized,
    Curated,
    Trusted
}
