using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dtos.Jupiter;

public sealed class SwapBuildRequestDto
{
  [Required] public string UserPublicKey { get; init; } = default!;
  /// <summary>Відповідь з /quote (повна). Передаємо як є.</summary>
  [Required] public QuoteResponseDto Quote { get; init; } = default!;
  /// <summary>Опції для /swap (необов’язково)</summary>
  public SwapOptions? Options { get; init; }
}