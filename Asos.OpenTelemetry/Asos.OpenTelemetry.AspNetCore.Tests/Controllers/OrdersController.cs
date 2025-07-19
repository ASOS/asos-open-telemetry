using Microsoft.AspNetCore.Mvc;

namespace Asos.OpenTelemetry.AspNetCore.Tests.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateOrder([FromBody] object order)
    {
        return Ok("Order created");
    }

    [HttpPost("invalid")]
    public IActionResult CreateInvalidOrder([FromBody] object order)
    {
        return BadRequest("Invalid order");
    }
}