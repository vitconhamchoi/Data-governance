namespace DataGovernance.Domain.Entities.Harness;

public class Memory : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public Guid ScopeId { get; set; }
    public MemoryType MemoryType { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
