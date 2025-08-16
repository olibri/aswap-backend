using Domain.Models.Api.CoinJelly;
using Domain.Models.DB;

namespace App.Mapper;

public static partial class CoinJellyMapper
{
  public static CoinJellyEntity ToEntity(CoinJellyDto dto)
  {
    return new CoinJellyEntity
    {
      CompanyWalletAddress = dto.CompanyWalletAddress,
      CryptoCurrency = dto.CryptoCurrency,
      CryptoCurrencyChain = dto.CryptoCurrencyChain
    };
  }

  public static CoinJellyAccountHistoryRequest ToApi(CoinJellyAccountHistoryEntity entity)
  {
    return new CoinJellyAccountHistoryRequest
    (
      entity.TxID,
      entity.CryptoSend,
      entity.CryptoGet,
      entity.AmountSend,
      entity.AmountGet,
      entity.FeeAtomic,
      entity.CreatedAtUtc,
      entity.Status,
      entity.UserWallet
    );
  }

  //public override NewUserCoinJellyRequest ToApi(CoinJellyAccountHistoryEntity entity)
  //{
  //  return new NewUserCoinJellyRequest
  //  (
      
  //  );
  //}
}