using DataGovernance.Application.Harness.Contracts;

namespace DataGovernance.Application.Harness.Abstractions;

public interface IToolHarnessService
{
    Task RegisterToolAsync(RegisterToolRequest request, CancellationToken cancellationToken);
    Task<ToolInvocationResult> InvokeAsync(ToolInvocationRequest request, CancellationToken cancellationToken);
}
