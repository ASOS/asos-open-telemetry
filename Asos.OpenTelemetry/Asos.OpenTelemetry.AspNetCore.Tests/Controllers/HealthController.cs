using Microsoft.AspNetCore.Mvc;

namespace Asos.OpenTelemetry.AspNetCore.Tests.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Health()
    {
        return Ok("Healthy");
    }

    [HttpGet("detailed")]
    public IActionResult DetailedHealth()
    {
        return Ok("Detailed health");
    }

    [HttpGet("exception")]
    public IActionResult HealthException()
    {
        throw new InvalidOperationException("Health check failed");
    }
}