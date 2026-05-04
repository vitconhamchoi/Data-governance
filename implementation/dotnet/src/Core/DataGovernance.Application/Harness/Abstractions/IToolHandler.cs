namespace DataGovernance.Application.Harness.Abstractions;

public interface IToolHandler
{
    string Name { get; }
    Task<string> ExecuteAsync(string inputJson, CancellationToken cancellationToken);
}
