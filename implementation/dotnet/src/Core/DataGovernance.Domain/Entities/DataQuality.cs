namespace DataGovernance.Domain.Entities;

/// <summary>
/// Represents a data quality rule and its execution results
/// </summary>
public class DataQualityRule : BaseEntity
{
    /// <summary>
    /// Name of the quality rule
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the rule checks
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data asset this rule applies to
    /// </summary>
    public Guid DataAssetId { get; set; }

    /// <summary>
    /// Category of quality rule
    /// </summary>
    public QualityRuleCategory Category { get; set; }

    /// <summary>
    /// Severity level if rule fails
    /// </summary>
    public Severity Severity { get; set; }

    /// <summary>
    /// Rule definition (SQL query, expression, etc.)
    /// </summary>
    public string RuleDefinition { get; set; } = string.Empty;

    /// <summary>
    /// Expected threshold for the rule (e.g., >95%)
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Whether the rule is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Schedule for running the rule (cron expression)
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    /// Last execution timestamp
    /// </summary>
    public DateTimeOffset? LastExecutedAt { get; set; }

    /// <summary>
    /// Last execution result
    /// </summary>
    public bool? LastExecutionPassed { get; set; }

    /// <summary>
    /// Last execution score
    /// </summary>
    public double? LastExecutionScore { get; set; }
}

/// <summary>
/// Data quality rule execution result
/// </summary>
public class DataQualityResult : BaseEntity
{
    /// <summary>
    /// Quality rule ID
    /// </summary>
    public Guid QualityRuleId { get; set; }

    /// <summary>
    /// Execution timestamp
    /// </summary>
    public DateTimeOffset ExecutedAt { get; set; }

    /// <summary>
    /// Whether the rule passed
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Quality score (0-100)
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Number of records evaluated
    /// </summary>
    public long TotalRecords { get; set; }

    /// <summary>
    /// Number of records that passed the rule
    /// </summary>
    public long PassedRecords { get; set; }

    /// <summary>
    /// Number of records that failed the rule
    /// </summary>
    public long FailedRecords { get; set; }

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Sample of failed records (JSON)
    /// </summary>
    public string? FailedRecordsSample { get; set; }

    /// <summary>
    /// Execution details and metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Category of data quality rule
/// </summary>
public enum QualityRuleCategory
{
    Completeness,
    Accuracy,
    Consistency,
    Validity,
    Uniqueness,
    Timeliness,
    Integrity
}

/// <summary>
/// Severity level
/// </summary>
public enum Severity
{
    Info,
    Warning,
    Error,
    Critical
}
