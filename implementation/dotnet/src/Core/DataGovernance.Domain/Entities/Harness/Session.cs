namespace DataGovernance.Domain.Entities.Harness;

public class Session : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public DateTimeOffset LastActivityAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Message> Messages { get; set; } = new();
    public List<Run> Runs { get; set; } = new();
}
