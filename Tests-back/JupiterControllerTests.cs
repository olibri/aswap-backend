using Aswap_back.Controllers;
using Domain.Models.Dtos.Jupiter;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class JupiterControllerTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  private readonly PlatformController _controller = fixture.GetService<PlatformController>();

  [Fact]
  public async Task GetJupiterQuote_Works()
  {
    // arrange
    var q = new QuoteQueryDto
    {
      InputMint = "So11111111111111111111111111111111111111112", // SOL
      OutputMint = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", // USDC
      Amount = 1_000_000, // 0.001 SOL
      SlippageBps = 50
    };

    // act
    var result = await _controller.GetJupiterQuote(q, default) as OkObjectResult;

    // assert
    result.ShouldNotBeNull();
    var dto = result.Value as QuoteResponseDto;
    dto.ShouldNotBeNull();
    dto!.InputMint.ShouldBe(q.InputMint);
    dto.OutputMint.ShouldBe(q.OutputMint);
    dto.SwapMode.ShouldBe("ExactIn");
    dto.InAmount.ShouldNotBeNullOrWhiteSpace();
    dto.OutAmount.ShouldNotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task BuildJupiterSwap_Works()
  {
    var q = new QuoteQueryDto
    {
      InputMint = "So11111111111111111111111111111111111111112",
      OutputMint = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
      Amount = 1_000_000
    };
    var quoteRes = (await _controller.GetJupiterQuote(q, default) as OkObjectResult)!.Value as QuoteResponseDto;

    var body = new SwapBuildRequestDto
    {
      UserPublicKey = "jdocuPgEAjMfihABsPgKEvYtsmMzjUHeq9LX4Hvs7f3",
      Quote = quoteRes!
    };

    // act
    var result = await _controller.BuildJupiterSwap(body, default) as OkObjectResult;

    // assert
    result.ShouldNotBeNull();
    var dto = result.Value as SwapResponseDto;
    dto.ShouldNotBeNull();
    dto!.SwapTransaction.ShouldNotBeNullOrWhiteSpace();
    dto.LastValidBlockHeight.ShouldBeGreaterThan(0ul);
  }
}