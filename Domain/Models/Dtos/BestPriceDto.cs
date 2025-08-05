using Domain.Models.Enums;

namespace Domain.Models.Dtos;

public sealed record BestPriceDto
{
  public Guid OrderId { get; init; }
  public OrderSide Side { get; init; }
  public ulong Price { get; init; }
  public decimal Available { get; init; }
  public IReadOnlyList<short> MethodIds { get; init; } = Array.Empty<short>();
}