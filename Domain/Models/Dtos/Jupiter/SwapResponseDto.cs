using System.Text.Json.Serialization;

namespace Domain.Models.Dtos.Jupiter;

public sealed class SwapResponseDto
{
  [JsonPropertyName("swapTransaction")] public string SwapTransaction { get; set; } = default!;
  [JsonPropertyName("lastValidBlockHeight")] public ulong LastValidBlockHeight { get; set; }
  [JsonPropertyName("prioritizationFeeLamports")] public ulong? PrioritizationFeeLamports { get; set; }
}