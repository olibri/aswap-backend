using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Rating;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Accounts;

public sealed class RatingService(P2PDbContext db) : IRatingService
{
  public async Task<long> AddReviewAsync(string fromWallet, AddReviewDto dto, CancellationToken ct)
  {
    if (fromWallet.Equals(dto.ToWallet, StringComparison.OrdinalIgnoreCase))
      throw new InvalidOperationException("Self-rating is not allowed.");

    var review = new RatingReviewEntity
    {
      ToWallet = dto.ToWallet,
      FromWallet = fromWallet,
      Score = dto.Score,
      Comment = dto.Comment,
      DealId = dto.DealId,
      CreatedAt = DateTime.UtcNow
    };

    db.Ratings.Add(review);
    await db.SaveChangesAsync(ct);
    return review.Id;
  }

  public Task<RatingReviewDto[]> GetReviewsAsync(string wallet, int skip, int take, CancellationToken ct)
  {
    return db.Ratings
      .Where(x => x.ToWallet == wallet)
      .OrderByDescending(x => x.CreatedAt)
      .Skip(skip).Take(take)
      .Select(x => new RatingReviewDto(x.Id, x.FromWallet, x.Score, x.Comment, x.CreatedAt, x.DealId))
      .ToArrayAsync(ct);
  }

  public async Task<RatingSummaryDto> GetSummaryAsync(string wallet, CancellationToken ct)
  {
    var q = db.Ratings.Where(r => r.ToWallet == wallet);

    var reviews = await q.CountAsync(ct);
    if (reviews == 0)
      return new RatingSummaryDto(wallet, 0, 0, 0, 0m);

    var positive = await q.CountAsync(r => r.Score >= 4, ct);
    var disputes = 0;
    var avg = await q.AverageAsync(r => r.Score, ct);

    return new RatingSummaryDto(wallet, reviews, positive, disputes, Math.Round((decimal)avg, 2));
  }
}