using Domain.Interfaces.CoinJelly;
using Domain.Models.Api.CoinJelly;
using Domain.Models.Api.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/coin-jelly")]
public class CoinJellyController(
  ICoinJellyService jellyService,
  ILogger<CoinJellyController> log) : Controller
{
  [HttpPost("create-jelly")]
  public async Task<IActionResult> CreateJelly([FromBody] NewUserCoinJellyRequest dto, CancellationToken ct)
  {
    var jelly = await jellyService.CreateNewJellyAsync(dto, ct);
    return Ok(jelly);
  }


  [HttpGet("jelly-history")]
  [ProducesResponseType(typeof(DashboardMetricsDto), 200)]
  public async Task<ActionResult<CoinJellyAccountHistoryRequest[]>> GetJellyHistory([FromQuery] string userWallet,
    CancellationToken ct)
  {
    log.LogInformation("Get jelly history (first render) {@Query}", userWallet);
    return Ok(await jellyService.GetJellyHistoryAsync(userWallet, ct));
  }

  [HttpGet("jelly-all-methods")]
  [ProducesResponseType(typeof(DashboardMetricsDto), 200)]
  public async Task<ActionResult<CoinJellyDto[]>> GetJellyAllMethods(CancellationToken ct)
  {
    log.LogInformation("Get jelly methods (first render)");
    return Ok(await jellyService.GetAllJellyMethodsAsync(ct));
  }
}