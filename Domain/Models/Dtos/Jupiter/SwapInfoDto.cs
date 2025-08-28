using System.Text.Json.Serialization;

namespace Domain.Models.Dtos.Jupiter;

public sealed class SwapInfoDto
{
  [JsonPropertyName("ammKey")] public string? AmmKey { get; set; }
  [JsonPropertyName("label")] public string? Label { get; set; }
  [JsonPropertyName("inputMint")] public string InputMint { get; set; } = default!;
  [JsonPropertyName("outputMint")] public string OutputMint { get; set; } = default!;
  [JsonPropertyName("inAmount")] public string InAmount { get; set; } = default!;
  [JsonPropertyName("outAmount")] public string OutAmount { get; set; } = default!;
  [JsonPropertyName("feeAmount")] public string? FeeAmount { get; set; }
  [JsonPropertyName("feeMint")] public string? FeeMint { get; set; }
}