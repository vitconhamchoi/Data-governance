namespace DataGovernance.Domain.Entities;

/// <summary>
/// Represents a governance policy
/// </summary>
public class GovernancePolicy : BaseEntity
{
    /// <summary>
    /// Policy name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Policy description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Policy type
    /// </summary>
    public PolicyType Type { get; set; }

    /// <summary>
    /// Policy definition (OPA Rego, JSON, etc.)
    /// </summary>
    public string PolicyDefinition { get; set; } = string.Empty;

    /// <summary>
    /// Policy enforcement mode
    /// </summary>
    public EnforcementMode EnforcementMode { get; set; }

    /// <summary>
    /// Scope of policy (global, tenant-specific, asset-specific)
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Whether the policy is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority (higher number = higher priority)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Effective date
    /// </summary>
    public DateTimeOffset EffectiveFrom { get; set; }

    /// <summary>
    /// Expiration date (null = no expiration)
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; set; }

    /// <summary>
    /// Data assets this policy applies to
    /// </summary>
    public List<Guid> ApplicableAssets { get; set; } = new();

    /// <summary>
    /// Tags for policy categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Policy violation record
/// </summary>
public class PolicyViolation : BaseEntity
{
    /// <summary>
    /// Policy that was violated
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Data asset involved
    /// </summary>
    public Guid? DataAssetId { get; set; }

    /// <summary>
    /// User or system that triggered the violation
    /// </summary>
    public string Actor { get; set; } = string.Empty;

    /// <summary>
    /// Action that was attempted
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of violation
    /// </summary>
    public DateTimeOffset ViolatedAt { get; set; }

    /// <summary>
    /// Severity of violation
    /// </summary>
    public Severity Severity { get; set; }

    /// <summary>
    /// Details of the violation
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Whether the action was blocked or just logged
    /// </summary>
    public bool WasBlocked { get; set; }

    /// <summary>
    /// Resolution status
    /// </summary>
    public ViolationStatus Status { get; set; }

    /// <summary>
    /// Resolution notes
    /// </summary>
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Resolved by
    /// </summary>
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// Resolved at
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }
}

/// <summary>
/// Type of governance policy
/// </summary>
public enum PolicyType
{
    Access,
    Retention,
    Classification,
    DataQuality,
    Privacy,
    Compliance,
    Usage,
    Encryption,
    Masking
}

/// <summary>
/// Policy enforcement mode
/// </summary>
public enum EnforcementMode
{
    /// <summary>
    /// Log violations but don't block
    /// </summary>
    Monitor,

    /// <summary>
    /// Block violations
    /// </summary>
    Enforce,

    /// <summary>
    /// Testing mode - log but don't affect production
    /// </summary>
    Audit
}

/// <summary>
/// Violation resolution status
/// </summary>
public enum ViolationStatus
{
    Open,
    InReview,
    Resolved,
    Accepted,
    FalsePositive
}
