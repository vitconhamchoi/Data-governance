using Microsoft.AspNetCore.Mvc;

namespace DataGovernance.API.Controllers;

/// <summary>
/// Health check endpoints for monitoring
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Liveness probe - checks if the application is running
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Readiness probe - checks if the application is ready to serve traffic
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Ready()
    {
        // Add checks for dependencies (database, cache, etc.)
        // For now, return OK
        return Ok(new
        {
            Status = "Ready",
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}
