using System.Text.Json.Serialization;

namespace Domain.Models.Dtos.Jupiter;

public sealed class RoutePlanStepDto
{
  [JsonPropertyName("swapInfo")] public SwapInfoDto SwapInfo { get; set; } = default!;
  [JsonPropertyName("percent")] public int Percent { get; set; }
}