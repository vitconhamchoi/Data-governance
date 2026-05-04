namespace DataGovernance.Domain.Entities.Harness;

public class Run : BaseEntity
{
    private static readonly Dictionary<RunStatus, HashSet<RunStatus>> AllowedTransitions = new()
    {
        [RunStatus.Pending] = [RunStatus.Classifying, RunStatus.Cancelled],
        [RunStatus.Classifying] = [RunStatus.Planning, RunStatus.Executing, RunStatus.Failed],
        [RunStatus.Planning] = [RunStatus.AwaitingApproval, RunStatus.Executing, RunStatus.Failed],
        [RunStatus.AwaitingApproval] = [RunStatus.Executing, RunStatus.Cancelled],
        [RunStatus.Executing] = [RunStatus.WaitingExternal, RunStatus.Synthesizing, RunStatus.Failed],
        [RunStatus.WaitingExternal] = [RunStatus.Executing, RunStatus.Failed],
        [RunStatus.Synthesizing] = [RunStatus.Completed, RunStatus.Failed],
        [RunStatus.Completed] = [],
        [RunStatus.Failed] = [],
        [RunStatus.Cancelled] = []
    };

    public Guid TenantId { get; set; }
    public Guid SessionId { get; set; }
    public string TaskType { get; set; } = "general";
    public string Strategy { get; set; } = "default";
    public string Model { get; set; } = "gpt-4o-mini";
    public string Provider { get; set; } = "openai";
    public RunStatus Status { get; private set; } = RunStatus.Pending;
    public string Input { get; set; } = string.Empty;
    public string? FinalOutput { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int LatencyMs { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public decimal CostUsd { get; set; }
    public string? LastError { get; set; }

    public Session? Session { get; set; }
    public List<RunStep> Steps { get; set; } = new();
    public List<Approval> Approvals { get; set; } = new();

    public bool TryTransitionTo(RunStatus newStatus, out string? error)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
        {
            error = $"Invalid transition from {Status} to {newStatus}";
            return false;
        }

        Status = newStatus;
        if (newStatus == RunStatus.Completed || newStatus == RunStatus.Failed || newStatus == RunStatus.Cancelled)
        {
            CompletedAt = DateTimeOffset.UtcNow;
        }

        error = null;
        return true;
    }

    public void SetStatusUnsafe(RunStatus status)
    {
        Status = status;
    }
}
