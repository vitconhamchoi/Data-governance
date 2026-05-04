using DataGovernance.Application.Harness.Abstractions;
using System.Text.Json;

namespace DataGovernance.API.Services.Harness.BuiltinTools;

public sealed class UtcNowToolHandler : IToolHandler
{
    public string Name => "utc_now";

    public Task<string> ExecuteAsync(string inputJson, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new { utcNow = DateTimeOffset.UtcNow });
        return Task.FromResult(payload);
    }
}
