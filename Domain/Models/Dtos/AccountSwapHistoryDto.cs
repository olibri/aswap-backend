namespace Domain.Models.Dtos;

public sealed class AccountSwapHistoryDto
{
  public string Tx { get; init; } = null!;
  public string CryptoFrom { get; init; } = null!;
  public string CryptoTo { get; init; } = null!;
  public decimal AmountIn { get; init; }
  public decimal AmountOut { get; init; }
  public decimal PriceUsdIn { get; init; }
  public decimal PriceUsdOut { get; init; }
  public DateTime CreatedAtUtc { get; init; }
}