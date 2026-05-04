using DataGovernance.Application.Harness.Abstractions;

namespace DataGovernance.API.Services.Harness;

public sealed class RunProcessorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RunProcessorBackgroundService> _logger;

    public RunProcessorBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RunProcessorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
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

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
