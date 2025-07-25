namespace Domain.Models.Api.Rating;

public record RatingSummaryDto(string Wallet, int Reviews, int Positive, int Disputes, decimal AvgScore);