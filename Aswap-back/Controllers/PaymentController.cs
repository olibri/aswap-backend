using Domain.Interfaces.Services.IP;
using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.Api.PaymentMethod;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentController(
  IPaymentCatalog catalog,
  IPopularityCounter counter,
  IGeoIpService geo,
  IClientIpAccessor ipAccessor) : Controller
{
  private const string GlobalRegion = "ZZ";

  [HttpGet]
  public async Task<ActionResult<PaymentResponse>> Get(
    [FromQuery] string? q,
    [FromQuery] string? region,
    CancellationToken ct)
  {
    var country = region
                  ?? geo.ResolveCountry(ipAccessor.GetClientIp())
                  ?? GlobalRegion;

    var popularIds = await counter.Top(country, 8, ct);
    var popular = catalog.All.Where(m => popularIds.Contains(m.Id)).ToList();
    var list = string.IsNullOrWhiteSpace(q)
      ? catalog.All
      : catalog.Search(q);

    return Ok(new PaymentResponse(popular, list));
  }
}