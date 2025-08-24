using Domain.Models.Api.CoinPrice;

namespace Domain.Interfaces.Services.CoinService.TokenRepo;

public interface IPriceSnapshotRepository
{
  Task UpsertMinuteAsync(IEnumerable<PriceSnapshotUpsertDto> rows, CancellationToken ct);

  Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct);

  Task<int> DeleteAllExceptDayAsync(DateOnly dayUtc, CancellationToken ct);
}