namespace Domain.Models.Dtos.Jupiter;

public sealed class SwapOptions
{
  public bool? DynamicComputeUnitLimit { get; init; } = true;

  /// <summary>Фіксована сума пріоритизації в лампортах (якщо хочеш сам контролювати).</summary>
  public ulong? PrioritizationFeeLamports { get; init; }

  /// <summary>Альтернатива: вибір рівня з maxLamports, як у прикладі Jupiter.</summary>
  public PriorityLevelWithMaxLamports? PriorityLevelWithMaxLamports { get; init; }
}