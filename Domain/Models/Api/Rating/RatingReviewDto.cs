namespace Domain.Models.Api.Rating;

public record RatingReviewDto(
  long Id,
  string FromWallet,
  decimal Score,
  string? Comment,
  DateTime CreatedAt,
  ulong? DealId);