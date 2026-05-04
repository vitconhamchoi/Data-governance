namespace DataGovernance.API.Configuration;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public string Transport { get; init; } = "RabbitMQ";
    public string RabbitMqHost { get; init; } = "localhost";
    public string RabbitMqUsername { get; init; } = "guest";
    public string RabbitMqPassword { get; init; } = "guest";
}
