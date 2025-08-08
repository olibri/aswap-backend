using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/platform")]
public class PlatformController(
    IMarketDbQueries marketDbQueries,
    ITgBotHandler tgBotHandler,
    IMarketDbCommand marketDbCommand,
    IChatDbCommand chatDbCommand,
    ILogger<PlatformController> log) : Controller
{
    [HttpPost]
    [Route("call-tg-bot")]
    public async Task<IActionResult> CallTgBot(TgBotDto tgBot)
    {
        log.LogInformation("Call bot request");
        await tgBotHandler.NotifyMessageAsync(tgBot);
        return Ok("Admin on the way");
    }

    [HttpGet("telegram-code")]
    public async Task<IActionResult> PostCode([FromQuery] string wallet)
    {
        var code = await chatDbCommand.GenerateCode(wallet);
        return Ok(new {code});
    }


    [HttpGet("check-order-status/{orderId}")]
    public async Task<IActionResult> CheckOrderStatus(ulong orderId)
    {
        var order = await marketDbQueries.CheckOrderStatusAsync(orderId);

        if (order is null)
            return NotFound();

        return Ok(new { isConfirmed = order.DealId });
    }


    [HttpGet("all-new-offers")]
    [ProducesResponseType(typeof(EscrowOrderDto[]), 200)]
    public async Task<IActionResult> GetAllNewOffers(
        [FromQuery] OffersQuery q, CancellationToken ct)
    {
        log.LogInformation("New offers requested {@Q}", q);
        var res = await marketDbQueries.GetAllNewOffersAsync(q, ct);
        return Ok(res);
    }



    [HttpGet]
    [Route("all-user-offers/{userId}")]
    [ProducesResponseType(typeof(List<EscrowOrderDto>), 200)]
    public async Task<IActionResult> GetAllUserOffers(string userId)
    {
        log.LogInformation("New user offers requested");
        var res = await marketDbQueries.GetAllUsersOffersAsync(userId);
        return Ok(res);
    }
}