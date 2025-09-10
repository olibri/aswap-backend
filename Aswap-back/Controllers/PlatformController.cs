using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Services.Account;
using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Api.CoinPrice;
using Domain.Models.Api.Order;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Api.Swap;
using Domain.Models.Dtos;
using Domain.Models.Dtos.Jupiter;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/platform")]
public class PlatformController(
  IMarketDbQueries marketDbQueries,
  ITgBotHandler tgBotHandler,
  ICoinService coinService,
  IChatDbCommand chatDbCommand,
  IJupiterSwapApi jupiter,
  ISwapService swapService,
  IUserTradingStatsService userStatsService,
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
    return Ok(new { code });
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
  [ProducesResponseType(typeof(PagedResult<EscrowOrderDto>), 200)]
  public async Task<IActionResult> GetAllNewOffers(
    [FromQuery] OffersQuery q, CancellationToken ct)
  {
    log.LogInformation("New offers requested {@Q}", q);
    var res = await marketDbQueries.GetAllNewOffersAsync(q, ct);
    return Ok(res);
  }


  [HttpGet("all-user-offers/{userId}")]
  [ProducesResponseType(typeof(PagedResult<EscrowOrderDto>), 200)]
  public async Task<IActionResult> GetAllUserOffers([FromRoute] string userId, [FromQuery] UserOffersQuery q)
  {
    log.LogInformation("User offers requested for {userId}", userId);
    var res = await marketDbQueries.GetAllUsersOffersAsync(userId, q);
    return Ok(res);
  }

  [HttpGet]
  [Route("coin-prices/{coinX}/{coinY}")]
  [ProducesResponseType(typeof(List<TokenDailyPriceResponse>), 200)]
  public async Task<IActionResult> GetCoinPrices(string coinX, string coinY)
  {
    log.LogInformation("Coin prices");
    var res = await coinService.GetPricesAsync(coinX, coinY);
    return Ok(res);
  }

  [HttpGet]
  [Route("coins")]
  [ProducesResponseType(typeof(List<TokenDailyPriceResponse>), 200)]
  public async Task<IActionResult> GetCoins(CancellationToken ct)
  {
    log.LogInformation("Coins");
    var res = await coinService.GetCoinsAsync(ct);
    return Ok(res);
  }

  [HttpGet("jupiter/quote")]
  [ProducesResponseType(typeof(QuoteResponseDto), 200)]
  public async Task<IActionResult> GetJupiterQuote([FromQuery] QuoteQueryDto q, CancellationToken ct)
  {
    if (!ModelState.IsValid) return ValidationProblem(ModelState);

    log.LogInformation("[JUP] Quote {Input}->{Output} amount={Amount} swapMode={Mode} slippageBps={Slip}",
      q.InputMint, q.OutputMint, q.Amount, q.SwapMode, q.SlippageBps);

    var quote = await jupiter.GetQuoteAsync(
      new QuoteRequest(q.InputMint, q.OutputMint, q.Amount, q.SwapMode, q.SlippageBps),
      ct);

    return Ok(quote);
  }

  [HttpPost("jupiter/swap")]
  [ProducesResponseType(typeof(SwapResponseDto), 200)]
  public async Task<IActionResult> BuildJupiterSwap([FromBody] SwapBuildRequestDto body, CancellationToken ct)
  {
    if (!ModelState.IsValid) return ValidationProblem(ModelState);

    log.LogInformation("[JUP] Swap build for {User}", body.UserPublicKey);
    var res = await swapService.AddSwapAsync(body, ct);
    return Ok(res);
  }

  [HttpGet("jup/swap/history")]
  [ProducesResponseType(typeof(PagedResult<AccountSwapHistoryDto>), 200)]
  public async Task<IActionResult> GetSwapHistory([FromQuery] SwapHistoryQuery q, CancellationToken ct)
  {
    log.LogInformation("Swap history for {userWallet}", q.UserWallet);
    var res = await swapService.SwapHistoryAsync(q, ct);
    return Ok(res);
  }

  [HttpGet("trading-stats/{wallet}")]
  public async Task<IActionResult> GetTradingStats(string wallet)
  {
    var stats = await userStatsService.GetUserStatsAsync(wallet);
    return Ok(stats);
  }
}