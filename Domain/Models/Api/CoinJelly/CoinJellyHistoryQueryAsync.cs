using Domain.Enums;

namespace Domain.Models.Api.CoinJelly;

public sealed record CoinJellyHistoryQueryAsync(
  int Page = 1,
  int Size = 25,
  JellySortField SortBy = JellySortField.CreatedAt,
  SortDir Dir = SortDir.Desc,
  string? UserWallet = null,
  CoinJellyStatus? Status = null,
  string? CryptoSend = null,
  string? CryptoGet = null,
  string? CryptoGetChain = null,
  DateTime? From = null,
  DateTime? To = null,
  decimal? AmountSendFrom = null,
  decimal? AmountSendTo = null
);