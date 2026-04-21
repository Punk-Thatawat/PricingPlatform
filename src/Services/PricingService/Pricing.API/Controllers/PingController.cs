using Microsoft.AspNetCore.Mvc;

namespace Pricing.API.Controllers;

[ApiController]
[Route("ping")]
public sealed class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "Pricing",
            message = "pong"
        });
    }
}
