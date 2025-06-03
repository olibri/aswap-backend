using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Interfaces.Database.Queries;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class UnitTest(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task QuickNodeCallback_ReturnsOk()
    {
        PostgresDatabase.ResetState("escrow_orders");

        //arrange
        var db = fixture.GetService<IMarketDbQueries>();
        var result = await OffersExtensions.CreateFakeOrder(fixture);

        //act
        var offer = await db.GetNewOfferAsync(1747314431853);

        //assert
        result.ShouldBeOfType<OkResult>();
        offer.ShouldNotBeNull();
        offer.DealId.ShouldBe(1747314431853UL);
        offer.FiatCode.ShouldBe("USD");
        offer.Amount.ShouldBe(1UL);
        offer.Price.ShouldBe(3m);
        offer.Status.ShouldBe(EscrowStatus.PendingOnChain);
        offer.BuyerFiat.ShouldBeNull();
        offer.OfferSide.ShouldBe(OrderSide.Sell);
    }

    [Fact]
    public async Task GetAllOffersTest()
    {
        PostgresDatabase.ResetState("escrow_orders");

        var ordersCount = 5;
        var controller = fixture.GetService<PlatformController>();
        await OffersExtensions.CreateFakeOrders(fixture, ordersCount);

        var result = await controller.GetAllNewOffers();
        
        result.ShouldNotBeNull();
        result.ShouldBeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult.Value.ShouldBeOfType<EscrowOrderDto[]>();
        var offers = okResult.Value as EscrowOrderDto[];
        offers.Length.ShouldBe(ordersCount);
    }

    [Fact]
    public async Task PartialUpdateOffer()
    {
        PostgresDatabase.ResetState("escrow_orders");
        var controller = fixture.GetService<PlatformController>();
        var marketDbQueries = fixture.GetService<IMarketDbQueries>();
        await OffersExtensions.CreateFakeOrder(fixture);

        var updateOrderDto = new UpdateOrderDto()
        {
            OrderId = 1747314431853UL,
            MaxFiatAmount = 10000,
            MinFiatAmount = 10,
            Status = EscrowStatus.OnChain,
            BuyerFiat = "wallet0xzzzz"
        };

        var result = await controller.UpdateOffers(updateOrderDto);
        result.ShouldNotBeNull();
        result.ShouldBeOfType<OkResult>();

        var updatedOrder = await marketDbQueries.GetAllNewOffersAsync();

        updatedOrder.ShouldNotBeNull();
        updatedOrder[0].MinFiatAmount.ShouldBe(10);
        updatedOrder[0].MaxFiatAmount.ShouldBe(10000);
        updatedOrder[0].Status.ShouldBe(EscrowStatus.OnChain);
        updatedOrder[0].BuyerFiat.ShouldBe("wallet0xzzzz");
    }
}