using Domain.Enums;

namespace Domain.Models.Api.Swap;

public sealed record SwapHistoryQuery(
  string UserWallet,
  DateTime? From,
  DateTime? To,
  SwapHistorySortField SortBy = SwapHistorySortField.CreatedAt,
  SortDir Dir = SortDir.Desc,
  int Page = 1,
  int Size = 20
);