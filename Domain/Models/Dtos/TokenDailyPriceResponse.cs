using System.Text.Json.Serialization;

namespace Domain.Models.Dtos;

public sealed class TokenDailyPriceResponse
{
  public string TokenMint { get; init; } = null!;

  public DateOnly DateUtc { get; init; }

  public IReadOnlyList<PricePointDto> Points { get; init; } = Array.Empty<PricePointDto>();

  public int Count => Points.Count;
  public DateTime? FirstCollectedUtc { get; init; }
  public DateTime? LastCollectedUtc { get; init; }
}