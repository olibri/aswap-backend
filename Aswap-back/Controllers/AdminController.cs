using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(
  IMarketDbQueries marketDbQueries,
  ITgBotHandler tgBotHandler,
  IMarketDbCommand marketDbCommand,
  IChatDbCommand chatDbCommand,
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

}