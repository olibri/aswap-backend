using Domain.Enums;
using Domain.Models.DB;

namespace App.Utils;

public static class TvlUtils
{
  /// <summary>
  /// Returns a map of «mint → TVL».
  /// </summary>
  public static Dictionary<string, decimal> Calculate(
    IEnumerable<EscrowOrderEntity> orders)
  {
    return orders
      .Where(o => o.EscrowStatus is EscrowStatus.OnChain or EscrowStatus.PartiallyOnChain)
      .GroupBy(o => o.TokenMint)
      .ToDictionary(
        g => g.Key!,
        g => g.Sum(x => (x.Amount ?? 0) - x.FilledQuantity));
  }

  /// <summary>
  /// TVL for a specific token.
  /// </summary>
  public static ulong Calculate(
    IEnumerable<EscrowOrderEntity> orders,
    string tokenMint)
  {
    return (ulong)Calculate(orders).GetValueOrDefault(tokenMint, 0);
  }
}