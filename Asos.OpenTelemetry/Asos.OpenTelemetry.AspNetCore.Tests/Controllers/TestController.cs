using Microsoft.AspNetCore.Mvc;

namespace Asos.OpenTelemetry.AspNetCore.Tests.Controllers;

/// <summary>
/// Test controllers for simulating different scenarios
/// </summary>
[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("server-error")]
    public IActionResult ServerError()
    {
        return StatusCode(500, "Internal Server Error");
    }

    [HttpGet("exception")]
    public IActionResult Exception([FromQuery] string type = "InvalidOperation")
    {
        throw type switch
        {
            "ArgumentNull" => new ArgumentNullException("Test parameter"),
            "Timeout" => new TimeoutException("Test timeout"),
            _ => new InvalidOperationException("Test exception")
        };
    }

    [HttpGet("slow")]
    public async Task<IActionResult> Slow([FromQuery] int delay = 1000)
    {
        await Task.Delay(delay);
        return Ok("Slow response");
    }

    [HttpGet("status/{code}")]
    public IActionResult Status(int code)
    {
        return StatusCode(code, $"Status {code}");
    }

    [HttpGet("performance/{id}")]
    public IActionResult Performance(int id)
    {
        return Ok($"Performance test {id}");
    }
}