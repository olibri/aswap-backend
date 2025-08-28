using System.Text.Json.Serialization;

namespace Domain.Models.Dtos.Jupiter;

public sealed class PlatformFeeDto
{
  [JsonPropertyName("amount")] public string? Amount { get; set; }
  [JsonPropertyName("feeBps")] public ushort? FeeBps { get; set; }
}