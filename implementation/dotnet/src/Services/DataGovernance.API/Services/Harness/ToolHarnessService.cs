using DataGovernance.API.Configuration;
using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.Application.Harness.Contracts;
using DataGovernance.Domain.Entities.Harness;
using DataGovernance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using System.Collections.Concurrent;

namespace DataGovernance.API.Services.Harness;

public sealed class ToolHarnessService : IToolHarnessService
{
    private readonly DataGovernanceDbContext _dbContext;
    private readonly IReadOnlyDictionary<string, IToolHandler> _toolHandlers;
    private readonly HarnessOptions _options;
    private readonly ConcurrentDictionary<string, AsyncPolicy<string>> _circuitBreakers = new(StringComparer.OrdinalIgnoreCase);

    public ToolHarnessService(
        DataGovernanceDbContext dbContext,
        IEnumerable<IToolHandler> toolHandlers,
        IOptions<HarnessOptions> options)
    {
        _dbContext = dbContext;
        _toolHandlers = toolHandlers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
    }

    public async Task RegisterToolAsync(RegisterToolRequest request, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ToolDefinitions
            .FirstOrDefaultAsync(x => x.TenantId == request.TenantId && x.Name == request.Name, cancellationToken);

        if (existing is null)
        {
            existing = new ToolDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                CreatedBy = "system",
                UpdatedBy = "system"
            };
            _dbContext.ToolDefinitions.Add(existing);
        }

        existing.Description = request.Description;
        existing.InputSchema = request.InputSchema;
        existing.OutputSchema = request.OutputSchema;
        existing.TimeoutSeconds = Math.Max(1, request.TimeoutSeconds);
        existing.RetryCount = Math.Max(0, request.RetryCount);
        existing.PermissionLevel = request.PermissionLevel;
        existing.SideEffectLevel = request.SideEffectLevel;
        existing.IsEnabled = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ToolInvocationResult> InvokeAsync(ToolInvocationRequest request, CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;

        var definition = await _dbContext.ToolDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.Context.TenantId && x.Name == request.ToolName && x.IsEnabled,
                cancellationToken);

        if (definition is null)
        {
            return new ToolInvocationResult(false, "{}", "tool_not_found", $"Tool '{request.ToolName}' not found.", DateTimeOffset.UtcNow - started);
        }

        if (request.Context.CallerPermissionLevel < definition.PermissionLevel)
        {
            return new ToolInvocationResult(false, "{}", "permission_denied", "Caller does not have permission to invoke this tool.", DateTimeOffset.UtcNow - started);
        }

        if (!_toolHandlers.TryGetValue(definition.Name, out var handler))
        {
            return new ToolInvocationResult(false, "{}", "tool_handler_missing", $"Tool handler '{definition.Name}' is not registered.", DateTimeOffset.UtcNow - started);
        }

        var timeoutSeconds = definition.TimeoutSeconds <= 0
            ? _options.Resilience.DefaultTimeoutSeconds
            : definition.TimeoutSeconds;

        var timeoutPolicy = Policy.TimeoutAsync<string>(TimeSpan.FromSeconds(timeoutSeconds));
        var circuitBreaker = _circuitBreakers.GetOrAdd(
            definition.Name,
            _ => Policy<string>
                .Handle<Exception>()
                .CircuitBreakerAsync(_options.Resilience.CircuitBreakerFailureCount, TimeSpan.FromSeconds(_options.Resilience.CircuitBreakerBreakSeconds)));
        var retryPolicy = Policy<string>
            .Handle<Exception>()
            .WaitAndRetryAsync(Math.Max(0, definition.RetryCount), attempt => TimeSpan.FromMilliseconds(200 * attempt));

        var policyWrap = Policy.WrapAsync(retryPolicy, circuitBreaker, timeoutPolicy);

        try
        {
            var output = await policyWrap.ExecuteAsync(async ct => await handler.ExecuteAsync(request.InputJson, ct), cancellationToken);
            return new ToolInvocationResult(true, output, null, null, DateTimeOffset.UtcNow - started);
        }
        catch (Exception ex)
        {
            return new ToolInvocationResult(false, "{}", "tool_execution_failed", ex.Message, DateTimeOffset.UtcNow - started);
        }
    }
}
