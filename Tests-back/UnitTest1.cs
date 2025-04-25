using Aswap_back.Controllers;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class UnitTest1(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CreateMarket()
    {
        //arrange
        var controller = fixture.GetService<MarketController>();
        var userWallet = "0x1111111111111SSSSSS_wallet";

        //act
        var fakeOrder = OrderFactory.CreateFakeOrderDto(userWallet);
        var result = await controller.CreateOrderAsync(fakeOrder);
        
        //assert1 
        result.ShouldNotBeNull();
        result.ShouldBeOfType<OkResult>();
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var order = okResult.Value.ShouldBeOfType<OrderDto>();

        //assert2
        order.ShouldNotBeNull();
        order.UserWallet.ShouldBe(userWallet);
        order.OrderType.ShouldBe(fakeOrder.OrderType);
        order.Currency.ShouldBe(fakeOrder.Currency);
        order.Amount.ShouldBe(fakeOrder.Amount);
        order.Price.ShouldBe(fakeOrder.Price);
    }
}