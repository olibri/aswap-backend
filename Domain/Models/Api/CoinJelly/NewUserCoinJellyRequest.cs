namespace Domain.Models.Api.CoinJelly;

public sealed record NewUserCoinJellyRequest(
  string CryptoCurrencyFromUser,
  string UserWallet,
  string[] NewUserWallets,
  string NewUserCrypto,
  string NewUserCryptoChain,
  decimal AmountSend,
  decimal AmountGet);