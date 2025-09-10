namespace Domain.Models.Api.User;

public sealed record UserTradingStatsDto(
  int TotalTrades,
  int BuyTradesCount,
  int SellTradesCount,
  int TradesLast30Days,
  decimal CompletionRate30D, // % від 0 до 100
  double AverageReleaseTimeSeconds,
  double AverageTradeTimeSeconds,
  DateTime JoinedDate
);