using DataGovernance.Application.Harness.Abstractions;
using DataGovernance.Application.Harness.Contracts;
using DataGovernance.Domain.Entities.Harness;

namespace DataGovernance.API.Features.Tools;

public static class ToolEndpoints
{
    public static IEndpointRouteBuilder MapToolEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tools").WithTags("Tools");

        group.MapPost("/register", async (
                RegisterToolBody body,
                HttpContext httpContext,
                IToolHarnessService toolHarnessService,
                CancellationToken cancellationToken) =>
            {
                if (!TryGetTenantId(httpContext, out var tenantId))
                {
                    return Results.BadRequest("Missing or invalid X-Tenant-Id header.");
                }

                await toolHarnessService.RegisterToolAsync(
                    new RegisterToolRequest(
                        tenantId,
                        body.Name,
                        body.Description,
                        body.InputSchema,
                        body.OutputSchema,
                        body.TimeoutSeconds,
                        body.RetryCount,
                        body.PermissionLevel,
                        body.SideEffectLevel),
                    cancellationToken);

                return Results.Accepted();
            })
            .WithName("RegisterTool");

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

    public sealed record RegisterToolBody(
        string Name,
        string Description,
        string InputSchema,
        string OutputSchema,
        int TimeoutSeconds,
        int RetryCount,
        ToolPermissionLevel PermissionLevel,
        ToolSideEffectLevel SideEffectLevel);
}
