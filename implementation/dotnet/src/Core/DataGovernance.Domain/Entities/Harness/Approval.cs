namespace DataGovernance.Domain.Entities.Harness;

public class Approval : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid RunId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string? ResolvedBy { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Requested;
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? Comment { get; set; }

    public Run? Run { get; set; }
}
