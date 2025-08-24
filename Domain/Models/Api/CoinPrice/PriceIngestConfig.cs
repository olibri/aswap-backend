namespace Domain.Models.Api.CoinPrice;

public sealed record PriceIngestConfig(
  string Quote,
  int PollEveryMinutes,
  int MaxIdsPerRequest,          // = 50
  int RequestsPerWindow,         // = 60
  TimeSpan Window                // = TimeSpan.FromMinutes(1)
);