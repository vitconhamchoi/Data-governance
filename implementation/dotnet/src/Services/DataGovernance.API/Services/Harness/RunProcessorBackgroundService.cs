using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.API.Configuration;
using Microsoft.Extensions.Options;

namespace DataGovernance.API.Services.Harness;

public sealed class RunProcessorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RunProcessorBackgroundService> _logger;
    private readonly HarnessOptions _options;

    public RunProcessorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RunProcessorBackgroundService> logger,
        IOptions<HarnessOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var harnessService = scope.ServiceProvider.GetRequiredService<IAIHarnessService>();
                await harnessService.ResumeInFlightRunsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resume in-flight runs.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.Processing.PollingIntervalSeconds)), stoppingToken);
        }
    }
}
