using App.Chat;
using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.TelegramBot;
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
    [HttpPut]
    [Route("update-offers")]
    public async Task<IActionResult> UpdateOffers(UpsertOrderDto orderUpdate)
    {
        log.LogInformation("Update order request");
        await marketDbCommand.UpdateCurrentOfferAsync(orderUpdate);
        return Ok();
    }

    [HttpPost]
    [Route("create-buyer-createOrder")]
    public async Task<IActionResult> CreateBuyerOffer(UpsertOrderDto createOrder)
    {
        log.LogInformation("Buyer createOrder request");
        await marketDbCommand.CreateBuyerOfferAsync(createOrder);
        return Ok();
    }

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


    [HttpGet]
    [Route("all-new-offers")]
    [ProducesResponseType(typeof(List<EscrowOrderDto>), 200)]
    public async Task<IActionResult> GetAllNewOffers()
    {
        log.LogInformation("New offers requested");
        var res = await marketDbQueries.GetAllNewOffersAsync();
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