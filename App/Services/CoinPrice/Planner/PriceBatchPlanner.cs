using Domain.Interfaces.Services.CoinService.Planner;
using Domain.Models.Api.CoinPrice;

namespace App.Services.CoinPrice.Planner;

public sealed class PriceBatchPlanner : IPriceBatchPlanner
{
  public PriceBatchPlan PlanBatches(IReadOnlyList<string> mints, PriceIngestConfig cfg)
  {
    var batches = new List<IReadOnlyList<string>>();
    for (var i = 0; i < mints.Count; i += cfg.MaxIdsPerRequest)
      batches.Add(mints.Skip(i).Take(cfg.MaxIdsPerRequest).ToArray());

    var delay = TimeSpan.FromTicks(cfg.Window.Ticks / Math.Max(1, cfg.RequestsPerWindow));
    return new PriceBatchPlan(batches, delay);
  }
}