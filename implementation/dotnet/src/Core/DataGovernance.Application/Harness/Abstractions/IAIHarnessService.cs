using DataGovernance.Application.Harness.Contracts;

namespace DataGovernance.Application.Harness.Abstractions;

public interface IAIHarnessService
{
    Task<RunDto> CreateRunAsync(CreateRunRequest request, CancellationToken cancellationToken);
    Task<RunDto?> GetRunAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RunStepDto>> GetRunStepsAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken);
    Task ProcessRunAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken);
    Task ResumeInFlightRunsAsync(CancellationToken cancellationToken);
}
