namespace Domain.Models.Api.Metrics;

public sealed record DashboardMetricsDto(
  IReadOnlyDictionary<string, decimal> TvlByAsset,
  IReadOnlyDictionary<string, int> OrdersSummary,
  double? AvgDealTimeSec,
  IReadOnlyDictionary<string, decimal> VolumeByAsset,
  UsersSnapshot Users,
  IpsSnapshot Ips,
  int OnlineUsers
);