using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Models.Api.PaymentMethod;
using Infrastructure.Migrations;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;
using Tests_back.Extensions.Payment;

namespace Tests_back;

public sealed class PaymentControllerTest(TestFixture fixture) : IClassFixture<TestFixture>
{
  private const string GlobalRegion = "ZZ";
  /// <summary>Returns all methods and highlights today's popular ones.</summary>
  [Fact]
  public async Task Get_Should_Return_All_And_Popular()
  {
    // Arrange
    fixture.ResetDb("payment_popularity_daily");             
    var method = await fixture.AnyMethodAsync();
    await fixture.AddPopularityAsync(method.Id, GlobalRegion, 5); 
    await fixture.ReloadCatalogAsync();

    var ctrl = fixture.GetService<PaymentController>()
                      .WithHttp(fixture);

    // Act
    var result = await ctrl.Get(null, CatalogKind.Payments, default);

    // Assert
    result.Result.ShouldBeOfType<OkObjectResult>();
    var payload = ((OkObjectResult)result.Result)
                  .Value.ShouldBeOfType<CatalogResponse>();

    payload.Payments.Popular.ShouldContain(p => p.Id == method.Id);
    payload.Payments.All.ShouldContain(p => p.Id == method.Id);
    payload.Payments.Popular.Count.ShouldBeLessThan(8);
  }

  /// <summary>Search filter returns only matching methods (case-insensitive).</summary>
  [Theory]
  [InlineData("visa")]      
  [InlineData("VISA")]      
  [InlineData("master")]    
  public async Task Get_With_Query_Filters_List(string query)
  {
    // Arrange
    await fixture.ReloadCatalogAsync();
    var ctrl = fixture.GetService<PaymentController>()
                      .WithHttp(fixture);

    // Act
    var result = await ctrl.Get(query, CatalogKind.Payments, default);

    // Assert
    var list = ((OkObjectResult)result.Result).Value
               .ShouldBeOfType<CatalogResponse>().Payments.All;

    list.ShouldAllBe(p => p.Name.Contains(query,
                       StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>If region param omitted – controller falls back to GeoIP.</summary>
  [Fact]
  public async Task Get_Without_Region_Uses_GeoIp_Fallback()
  {
    // Arrange
    var method = await fixture.AnyMethodAsync();
    await fixture.AddPopularityAsync(method.Id, "PL", 3);
    await fixture.ReloadCatalogAsync();

    // IP 95.158… → UA (GeoIP db)
    var ctrl = fixture.GetService<PaymentController>()
                      .WithHttp(fixture, ip: "5.173.151.43");

    // Act
    var result = await ctrl.Get(null, CatalogKind.Payments, default);

    // Assert
    var popular = ((OkObjectResult)result.Result).Value
                  .ShouldBeOfType<CatalogResponse>().Payments.Popular;

    popular.ShouldContain(p => p.Id == method.Id);
  }
}