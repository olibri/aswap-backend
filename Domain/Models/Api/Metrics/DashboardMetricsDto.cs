namespace Domain.Models.Api.Metrics;

public sealed record DashboardMetricsDto(
  IReadOnlyDictionary<string, int> OrdersSummary,
  double? AvgDealTimeSec,
  IReadOnlyDictionary<string, decimal> VolumeByAsset,
  UsersSnapshot Users,
  IpsSnapshot Ips,
  int OnlineUsers,
  IReadOnlyList<Series> Tvl7d
);

public sealed record TimePoint(long T, decimal V);
public sealed record Series(string Key, IReadOnlyList<TimePoint> Points);