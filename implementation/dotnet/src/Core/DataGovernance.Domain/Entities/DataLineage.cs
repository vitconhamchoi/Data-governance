namespace DataGovernance.Domain.Entities;

/// <summary>
/// Represents data lineage relationship between data assets
/// </summary>
public class DataLineage : BaseEntity
{
    /// <summary>
    /// Source data asset ID
    /// </summary>
    public Guid SourceAssetId { get; set; }

    /// <summary>
    /// Target data asset ID
    /// </summary>
    public Guid TargetAssetId { get; set; }

    /// <summary>
    /// Type of lineage relationship
    /// </summary>
    public LineageType Type { get; set; }

    /// <summary>
    /// Process or job that created the lineage
    /// </summary>
    public string Process { get; set; } = string.Empty;

    /// <summary>
    /// Transformation logic applied (SQL, code, etc.)
    /// </summary>
    public string? Transformation { get; set; }

    /// <summary>
    /// Column-level lineage mappings
    /// </summary>
    public List<ColumnMapping> ColumnMappings { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Column-level lineage mapping
/// </summary>
public class ColumnMapping
{
    public string SourceColumn { get; set; } = string.Empty;
    public string TargetColumn { get; set; } = string.Empty;
    public string? Transformation { get; set; }
}

/// <summary>
/// Type of lineage relationship
/// </summary>
public enum LineageType
{
    DirectCopy,
    Transformation,
    Aggregation,
    Join,
    Filter,
    Union,
    Derivation
}
