using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Services.IP;
using Domain.Interfaces.Services.Order;
using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.Api.Order;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrderController(
  IMarketDbQueries marketDbQueries,
  IMarketDbCommand marketDbCommand,
  IGeoIpService geo,
  IPopularityCounter pop,
  IClientIpAccessor ipAccessor,
  IBestPriceService IBestPriceService,
  ILogger<OrderController> log) : Controller
{
  [HttpPut]
  [Route("update-offers")]
  public async Task<IActionResult> UpdateOffers(UpsertOrderDto orderUpdate)
  {
    log.LogInformation("Update order request");
    await marketDbCommand.UpdateCurrentOfferAsync(orderUpdate);

    var region = geo.ResolveCountry(ipAccessor.GetClientIp()) ?? "ZZ";
    pop.Hit(orderUpdate.PaymentMethodId, region);

    return Ok();
  }

  [HttpPost]
  [Route("create-buyer-createOrder")]
  public async Task<IActionResult> CreateBuyerOffer(UpsertOrderDto createOrder)
  {
    log.LogInformation("Buyer createOrder request");
    await marketDbCommand.CreateBuyerOfferAsync(createOrder);

    var region = geo.ResolveCountry(ipAccessor.GetClientIp()) ?? "ZZ";
    pop.Hit(createOrder.PaymentMethodId, region);

    return Ok();
  }

  [HttpGet]
  [Route("get-best-price")]
  public async Task<IActionResult> GetBestPrice(BestPriceRequest bestPrice)
  {
    log.LogInformation("Buyer createOrder request");
    var res = await IBestPriceService.GetBestPriceAsync(bestPrice, CancellationToken.None);

    return Ok(res);
  }
}