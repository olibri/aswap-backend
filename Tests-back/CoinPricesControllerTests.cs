using Aswap_back.Controllers;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;

namespace Tests_back;

public class CoinPricesControllerTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task CoinPrices_Ok()
  {
    var ctrl = fixture.GetService<PlatformController>().WithHttp(fixture);
    var ok = (OkObjectResult)await ctrl.GetCoinPrices("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
      "1unatWhTyNHUBES9FCjZ5m2jmopTTs8RMk7bzzdvnZ8");

    ((TokenDailyPriceResponse[])ok.Value!).Length.ShouldBe(2);
  }
}