using Aswap_back.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Tests_back.Extensions;

public static class OffersExtensions
{
    public static async Task<IActionResult> CreateFakeOrder(TestFixture fixture)
    {
        var controller = fixture.GetService<WebHookController>();

        controller.SetJsonBody(TestJson.OfferInitialized);
        var token = new CancellationToken();

        var result = await controller.QuickNodeCallback(token);

        return result;
    }

    public static async Task CreateFakeOrders(TestFixture fixture, int ordersCount)
    {
        for(int i = 0; i < ordersCount; i++)
        {
            await CreateFakeOrder(fixture);
        }
    } 
}