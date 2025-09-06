using Domain.Models.DB;
using Domain.Models.Dtos;

namespace App.Mapper;

public static class AccountSwapHistoryMapping
{
  public static AccountSwapHistoryDto ToDto(this AccountSwapHistoryEntity e)
  {
    return new AccountSwapHistoryDto
    {
      Tx = e.Tx,
      CryptoFrom = e.CryptoFrom,
      CryptoTo = e.CryptoTo,
      AmountIn = e.AmountIn,
      AmountOut = e.AmountOut,
      PriceUsdIn = e.PriceUsdIn,
      PriceUsdOut = e.PriceUsdOut,
      CreatedAtUtc = e.CreatedAtUtc
    };
  }
}