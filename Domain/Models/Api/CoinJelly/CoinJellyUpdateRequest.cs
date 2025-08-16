using Domain.Enums;

namespace Domain.Models.Api.CoinJelly;

public sealed record CoinJellyUpdateRequest(
  Guid Id,
  string TxID,
  CoinJellyStatus Status
);