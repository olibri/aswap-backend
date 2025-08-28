using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Models.Dtos.Jupiter;

public sealed class QuoteQueryDto
{
  [Required] public string InputMint { get; init; } = default!;
  [Required] public string OutputMint { get; init; } = default!;
  /// <summary>Raw uint64 до декімалів токена.</summary>
  [Range(1, ulong.MaxValue)]
  public ulong Amount { get; init; }

  public SwapMode SwapMode { get; init; } = SwapMode.ExactIn;
  /// <summary>bps, напр. 50 = 0.5%</summary>
  public ushort SlippageBps { get; init; } = 50;
}