using Aswap_back.Controllers;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class UnitTest(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task QuickNodeCallback_ReturnsOk()
    {
        //arrange
        var controller = fixture.GetService<WebHookController>();
        controller.SetJsonBody(TestJson.OfferInitialized);

        //act
        var result = await controller.QuickNodeCallback();

        //assert
        result.ShouldBeOfType<OkResult>();
    }
}