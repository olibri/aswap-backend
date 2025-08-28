using System.Text.Json.Serialization;

namespace Domain.Models.Dtos.Jupiter;

public sealed class QuoteResponseDto
{
  [JsonPropertyName("inputMint")] public string InputMint { get; set; } = default!;
  [JsonPropertyName("inAmount")] public string InAmount { get; set; } = default!;
  [JsonPropertyName("outputMint")] public string OutputMint { get; set; } = default!;
  [JsonPropertyName("outAmount")] public string OutAmount { get; set; } = default!;
  [JsonPropertyName("otherAmountThreshold")] public string OtherAmountThreshold { get; set; } = default!;
  [JsonPropertyName("swapMode")] public string SwapMode { get; set; } = default!;
  [JsonPropertyName("slippageBps")] public ushort SlippageBps { get; set; }
  [JsonPropertyName("platformFee")] public PlatformFeeDto? PlatformFee { get; set; }
  [JsonPropertyName("priceImpactPct")] public string? PriceImpactPct { get; set; }
  [JsonPropertyName("routePlan")] public RoutePlanStepDto[] RoutePlan { get; set; } = Array.Empty<RoutePlanStepDto>();
  [JsonPropertyName("contextSlot")] public ulong? ContextSlot { get; set; }
  [JsonPropertyName("timeTaken")] public double? TimeTaken { get; set; }
}