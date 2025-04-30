using System.Text.Json.Serialization;
using System.Text.Json;

namespace Domain.Models.Api.Hooks;

public class SolanaWebhookPayload
{
    [JsonPropertyName("data")]
    public List<SolanaEventChunk> Data { get; set; } = [];

    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }          // ← не парсимо докладно
}