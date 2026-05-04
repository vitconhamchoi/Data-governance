namespace DataGovernance.Domain.Entities.Harness;

public class RunStep : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid RunId { get; set; }
    public int StepNo { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string? ToolName { get; set; }
    public RunStepStatus Status { get; set; } = RunStepStatus.Pending;
    public string? Input { get; set; }
    public string? Output { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public Run? Run { get; set; }
}
