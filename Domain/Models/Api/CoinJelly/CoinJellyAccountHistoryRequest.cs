using Domain.Enums;

namespace Domain.Models.Api.CoinJelly;

public sealed record CoinJellyAccountHistoryRequest(
  string TxID,
  string CryptoSend,
  string CryptoGet,
  decimal AmountSend,
  decimal AmountGet,
  decimal FeeAtomic,
  DateTime CreatedAtUtc,
  CoinJellyStatus Status,
  string UserWallet
);