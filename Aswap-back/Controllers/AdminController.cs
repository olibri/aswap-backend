using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Services;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Api.Auth;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(
  IMarketDbQueries marketDbQueries,
  IAccountService accounts,
  ILogger<PlatformController> log) : Controller
{
  //TODO: add jwt checker to this method 
  [HttpGet]
  [Route("all-admin-offers")]
  [ProducesResponseType(typeof(List<EscrowOrderDto>), 200)]
  public async Task<IActionResult> GetAllAdminOffers()
  {
    log.LogInformation("New offers requested");
    var res = await marketDbQueries.GetAllAdminOffersAsync();
    return Ok(res);
  }

  [Authorize(Roles = "admin")]
  [HttpPost("ban")]
  public async Task<IActionResult> BanUser([FromBody] BanUserDto dto, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(dto.Wallet)) return BadRequest("wallet required");
    var id = await accounts.BanAsync(dto, ct);
    return Ok(new { id });
  }
}