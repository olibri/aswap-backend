using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Rating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/rating")]
public sealed class RatingController(IRatingService ratings) : Controller
{
  [Authorize]
  [HttpPost]
  public async Task<IActionResult> Add([FromBody] AddReviewDto dto, CancellationToken ct)
  {
    var fromWallet = User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException();
    var id = await ratings.AddReviewAsync(fromWallet, dto, ct);
    return Ok(new { id });
  }

  [HttpGet("{wallet}/reviews")]
  public Task<RatingReviewDto[]> GetReviews(string wallet, int skip = 0, int take = 20, CancellationToken ct = default)
    => ratings.GetReviewsAsync(wallet, skip, take, ct);

  [HttpGet("{wallet}/summary")]
  public Task<RatingSummaryDto> GetSummary(string wallet, CancellationToken ct)
    => ratings.GetSummaryAsync(wallet, ct);
}