namespace Domain.Interfaces.Metrics;

public interface IPeriodicTask
{
  int IntervalSeconds { get; }

  Task ExecuteAsync(CancellationToken ct);
}