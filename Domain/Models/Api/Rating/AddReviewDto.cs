namespace Domain.Models.Api.Rating;

public record AddReviewDto(string ToWallet, decimal Score, string? Comment, ulong? DealId);