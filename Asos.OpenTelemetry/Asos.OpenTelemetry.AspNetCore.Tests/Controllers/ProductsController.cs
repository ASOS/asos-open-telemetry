using Microsoft.AspNetCore.Mvc;

namespace Asos.OpenTelemetry.AspNetCore.Tests.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        return Ok($"Product {id}");
    }
}