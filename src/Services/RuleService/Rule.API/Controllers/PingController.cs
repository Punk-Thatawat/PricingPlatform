using Microsoft.AspNetCore.Mvc;

namespace Rule.API.Controllers;

[ApiController]
[Route("ping")]
public sealed class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "Rule",
            message = "pong"
        });
    }
}
