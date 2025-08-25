namespace Domain.Models.Dtos;

public sealed class PricePointDto
{
  public DateTimeOffset Time { get; init; }   // ISO-8601 UTC
  public decimal Price { get; init; }
}