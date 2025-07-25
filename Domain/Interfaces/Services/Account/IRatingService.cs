using Domain.Models.Api.Rating;

namespace Domain.Interfaces.Services.Account;

public interface IRatingService
{
  Task<long> AddReviewAsync(string fromWallet, AddReviewDto dto, CancellationToken ct);
  Task<RatingReviewDto[]> GetReviewsAsync(string wallet, int skip, int take, CancellationToken ct);
  Task<RatingSummaryDto> GetSummaryAsync(string wallet, CancellationToken ct);

}