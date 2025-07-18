using Domain.Interfaces.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Metrics.BackgroundWorker;

public class SchedulerService(IEnumerable<IPeriodicTask> tasks,
  ILogger<SchedulerService> log)
  : BackgroundService
{
  private record TaskState(IPeriodicTask Task, int Remaining);

  protected override async Task ExecuteAsync(CancellationToken ct)
  {
    var list = tasks.Select(t => new TaskState(t, 0)).ToList();

    while (!ct.IsCancellationRequested)
    {
      for (var i = 0; i < list.Count; i++)
      {
        var state = list[i];

        if (state.Remaining <= 0)
        {
          _ = RunTask(state, ct);
          state = state with { Remaining = state.Task.IntervalSeconds };
        }
        else
        {
          state = state with { Remaining = state.Remaining - 1 };
        }

        list[i] = state;
      }

      await Task.Delay(1000, ct);        
    }
  }
  private async Task RunTask(TaskState state, CancellationToken ct)
  {
    try { await state.Task.ExecuteAsync(ct); }
    catch (Exception ex) { log.LogError(ex, "Task {T} failed", state.Task.GetType().Name); }
  }
}