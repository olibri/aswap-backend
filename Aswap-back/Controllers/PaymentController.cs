using Domain.Enums;
using Domain.Interfaces.Services.IP;
using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.Api.PaymentMethod;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentController(
  IPaymentCatalog catalog,
  ICurrencyCatalog currencyCat,
  IPopularityCounter counter,
  IGeoIpService geo,
  IClientIpAccessor ipAccessor) : Controller
{
  private const string GlobalRegion = "ZZ";

  [HttpGet]
  public async Task<ActionResult<CatalogResponse>> Get(
    [FromQuery] string? q,
    [FromQuery] CatalogKind type,
    CancellationToken ct)
  {
    var wantPay = type is CatalogKind.Payments or CatalogKind.All;
    var wantCurr = type is CatalogKind.Currencies or CatalogKind.All;

    IReadOnlyList<PaymentDto> popular = Array.Empty<PaymentDto>();
    IReadOnlyList<PaymentDto> list = Array.Empty<PaymentDto>();

    if (wantPay)
    {
      var country = geo.ResolveCountry(ipAccessor.GetClientIp()) ?? GlobalRegion;
      var popularIds = await counter.Top(country, 8, ct);

      popular = catalog.All.Where(m => popularIds.Contains(m.Id)).ToList();
      list = string.IsNullOrWhiteSpace(q) ? catalog.All
        : catalog.Search(q);
    }

    IReadOnlyList<CurrencyDto> currencies = Array.Empty<CurrencyDto>();
    if (wantCurr)
    {
      currencies = string.IsNullOrWhiteSpace(q) ? currencyCat.All
        : currencyCat.Search(q);
    }

    var result = new CatalogResponse(
      new PaymentResponse(popular, list),
      currencies);

    return Ok(new PaymentResponse(popular, list));
  }
}