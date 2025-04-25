namespace Tests_back.Extensions;

public static class OrderFactory
{
    public static Domain.Models.Dtos.CreateOrderDto CreateFakeOrderDto(string userId)
    {
        return new Domain.Models.Dtos.CreateOrderDto(
            Domain.Enums.OrderTypes.Buy,
            "BTC",
            1,
            1000,
            userId
        );
    }
}