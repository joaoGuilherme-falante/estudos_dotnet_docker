using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PingController : ControllerBase
{
    private readonly IPingService _pingService;

    public PingController(IPingService pingService)
    {
        _pingService = pingService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = _pingService.GetMessage() });
    }
}