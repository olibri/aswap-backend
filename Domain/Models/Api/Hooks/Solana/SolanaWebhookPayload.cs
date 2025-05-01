using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Models.Api.Hooks.Solana;

public class SolanaWebhookPayload
{
    [JsonPropertyName("data")] public List<SolanaEventChunk> Data { get; set; } = [];

    [JsonPropertyName("metadata")] public JsonElement? Metadata { get; set; }
}