namespace Domain.Models.Dtos.Jupiter;

public sealed class PriorityLevelWithMaxLamports
{
  public ulong MaxLamports { get; init; }
  public string PriorityLevel { get; init; } = "high";
}