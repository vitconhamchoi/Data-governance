using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.Application.Harness.Contracts;
using DataGovernance.Domain.Entities.Harness;
using System.Text.Json;

namespace DataGovernance.API.Features.Runs;

public static class RunEndpoints
{
    public static IEndpointRouteBuilder MapRunEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/runs").WithTags("Runs");

        group.MapPost("/", async (
                CreateRunBody body,
                HttpContext httpContext,
                IAIHarnessService harnessService,
                CancellationToken cancellationToken) =>
            {
                if (!TryGetTenantId(httpContext, out var tenantId))
                {
                    return Results.BadRequest("Missing or invalid X-Tenant-Id header.");
                }

                var sessionId = body.SessionId ?? Guid.NewGuid();

                var run = await harnessService.CreateRunAsync(
                    new CreateRunRequest(
                        tenantId,
                        sessionId,
                        body.Input,
                        body.Model ?? "gpt-4o-mini",
                        body.Provider ?? "openai",
                        body.TaskType,
                        body.Strategy),
                    cancellationToken);

                return Results.Created($"/api/runs/{run.Id}", run);
            })
            .WithName("CreateRun");

        group.MapGet("/{id:guid}", async (
                Guid id,
                HttpContext httpContext,
                IAIHarnessService harnessService,
                CancellationToken cancellationToken) =>
            {
                if (!TryGetTenantId(httpContext, out var tenantId))
                {
                    return Results.BadRequest("Missing or invalid X-Tenant-Id header.");
                }

                var run = await harnessService.GetRunAsync(tenantId, id, cancellationToken);
                return run is null ? Results.NotFound() : Results.Ok(run);
            })
            .WithName("GetRun");

        group.MapGet("/{id:guid}/steps", async (
                Guid id,
                HttpContext httpContext,
                IAIHarnessService harnessService,
                CancellationToken cancellationToken) =>
            {
                if (!TryGetTenantId(httpContext, out var tenantId))
                {
                    return Results.BadRequest("Missing or invalid X-Tenant-Id header.");
                }

                var steps = await harnessService.GetRunStepsAsync(tenantId, id, cancellationToken);
                return Results.Ok(steps);
            })
            .WithName("GetRunSteps");

        group.MapGet("/{id:guid}/events", async (
                Guid id,
                HttpContext context,
                IAIHarnessService harnessService,
                CancellationToken cancellationToken) =>
            {
                if (!TryGetTenantId(context, out var tenantId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Missing or invalid X-Tenant-Id header.", cancellationToken);
                    return;
                }

                context.Response.Headers.ContentType = "text/event-stream";

                while (!cancellationToken.IsCancellationRequested)
                {
                    var run = await harnessService.GetRunAsync(tenantId, id, cancellationToken);
                    if (run is null)
                    {
                        await context.Response.WriteAsync("event: error\ndata: run_not_found\n\n", cancellationToken);
                        await context.Response.Body.FlushAsync(cancellationToken);
                        return;
                    }

                    var payload = JsonSerializer.Serialize(new { run.Id, status = run.Status.ToString(), run.FinalOutput, run.LastError });
                    await context.Response.WriteAsync($"event: run_update\ndata: {payload}\n\n", cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);

                    if (run.Status is RunStatus.Completed or RunStatus.Failed or RunStatus.Cancelled)
                    {
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            })
            .WithName("StreamRunEvents");

        return app;
    }

    private static bool TryGetTenantId(HttpContext httpContext, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (!httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var values))
        {
            return false;
        }

        return Guid.TryParse(values.FirstOrDefault(), out tenantId);
    }

    public sealed record CreateRunBody(
        Guid? SessionId,
        string Input,
        string? Model,
        string? Provider,
        string? TaskType,
        string? Strategy);
}
