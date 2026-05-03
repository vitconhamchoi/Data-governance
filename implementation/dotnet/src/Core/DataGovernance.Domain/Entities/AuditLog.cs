namespace DataGovernance.Domain.Entities;

/// <summary>
/// Represents an audit log entry for data access and operations
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Event type (Read, Write, Delete, Update, etc.)
    /// </summary>
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// Resource type (DataAsset, Policy, User, etc.)
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Resource ID
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// Resource name or identifier
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Actor (user or service) who performed the action
    /// </summary>
    public string Actor { get; set; } = string.Empty;

    /// <summary>
    /// Actor type (User, Service, System)
    /// </summary>
    public string ActorType { get; set; } = string.Empty;

    /// <summary>
    /// Action performed
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Result of the action
    /// </summary>
    public AuditResult Result { get; set; }

    /// <summary>
    /// Source IP address
    /// </summary>
    public string? SourceIp { get; set; }

    /// <summary>
    /// User agent or client information
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Session ID
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Request ID for correlation
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Details of the operation (JSON)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Before state (for updates/deletes)
    /// </summary>
    public string? BeforeState { get; set; }

    /// <summary>
    /// After state (for creates/updates)
    /// </summary>
    public string? AfterState { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Metadata and additional context
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Type of audit event
/// </summary>
public enum AuditEventType
{
    DataAccess,
    DataModification,
    PolicyChange,
    UserAuthentication,
    UserAuthorization,
    ConfigurationChange,
    SystemEvent
}

/// <summary>
/// Result of audited action
/// </summary>
public enum AuditResult
{
    Success,
    Failure,
    Denied,
    Error
}
