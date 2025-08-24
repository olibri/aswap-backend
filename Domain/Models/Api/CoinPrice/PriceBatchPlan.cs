namespace Domain.Models.Api.CoinPrice;

public sealed record PriceBatchPlan(
  IReadOnlyList<IReadOnlyList<string>> Batches,
  TimeSpan DelayBetweenRequests
);