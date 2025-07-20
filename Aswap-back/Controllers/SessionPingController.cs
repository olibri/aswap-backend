using Domain.Interfaces.Services;
using Domain.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/session")]
public class SessionPingController(ISessionService sessions) : Controller
{
  [HttpPost("start")]
  public Task<IActionResult> Start([FromBody] SessionPingDto dto,
    CancellationToken ct)
  {
    return Execute(sessions.StartAsync, dto, ct);
  }

  [HttpPost("ping")]
  public Task<IActionResult> Ping([FromBody] SessionPingDto dto,
    CancellationToken ct)
  {
    return Execute(sessions.PingAsync, dto, ct);
  }

  private async Task<IActionResult> Execute(
    Func<SessionPingDto, CancellationToken, Task> action,
    SessionPingDto dto,
    CancellationToken ct)
  {
    await action(dto, ct);
    return Ok();
  }
}