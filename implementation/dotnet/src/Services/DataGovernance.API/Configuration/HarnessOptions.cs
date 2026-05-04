namespace DataGovernance.API.Configuration;

public sealed class HarnessOptions
{
    public const string SectionName = "Harness";

    public OpenAIOptions OpenAI { get; init; } = new();
    public AnthropicOptions Anthropic { get; init; } = new();
    public ResilienceOptions Resilience { get; init; } = new();

    public sealed class OpenAIOptions
    {
        public string ApiKey { get; init; } = string.Empty;
        public string Model { get; init; } = "gpt-4o-mini";
    }

    public sealed class AnthropicOptions
    {
        public string ApiKey { get; init; } = string.Empty;
        public string Model { get; init; } = "claude-3-5-sonnet-latest";
        public string Endpoint { get; init; } = "https://api.anthropic.com";
    }

    public sealed class ResilienceOptions
    {
        public int DefaultTimeoutSeconds { get; init; } = 30;
        public int CircuitBreakerFailureCount { get; init; } = 5;
        public int CircuitBreakerBreakSeconds { get; init; } = 30;
    }
}
