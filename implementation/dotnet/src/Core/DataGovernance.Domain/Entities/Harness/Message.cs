namespace DataGovernance.Domain.Entities.Harness;

public class Message : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SessionId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;

    public Session? Session { get; set; }
}
