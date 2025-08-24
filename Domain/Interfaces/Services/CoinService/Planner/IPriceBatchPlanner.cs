using Domain.Models.Api.CoinPrice;

namespace Domain.Interfaces.Services.CoinService.Planner;

public interface IPriceBatchPlanner
{
  PriceBatchPlan PlanBatches(
    IReadOnlyList<string> mints,
    PriceIngestConfig config);
}