namespace DataGovernance.Domain.Entities.Harness;

public class ToolDefinition : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InputSchema { get; set; } = "{}";
    public string OutputSchema { get; set; } = "{}";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 2;
    public bool IsEnabled { get; set; } = true;
    public ToolSideEffectLevel SideEffectLevel { get; set; } = ToolSideEffectLevel.Read;
    public ToolPermissionLevel PermissionLevel { get; set; } = ToolPermissionLevel.Public;
}
