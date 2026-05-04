using DataGovernance.Application.Harness.Abstractions;

namespace DataGovernance.API.Services.Harness.BuiltinTools;

public sealed class EchoToolHandler : IToolHandler
{
    public string Name => "echo";

    public Task<string> ExecuteAsync(string inputJson, CancellationToken cancellationToken)
    {
        return Task.FromResult(inputJson);
    }
}
