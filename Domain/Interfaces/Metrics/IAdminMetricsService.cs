using Domain.Models.Api.Metrics;

namespace Domain.Interfaces.Metrics;

public interface IAdminMetricsService
{
  Task<DashboardMetricsDto> GetDashboardAsync(DashboardQuery q, CancellationToken ct);
}