using System.Text.Json.Serialization;

namespace Domain.Models.Api.Hooks.Solana;

public class SolanaEventChunk
{
    [JsonPropertyName("blockTime")]
    public long BlockTime { get; set; }

    [JsonPropertyName("logs")]
    public string[] Logs { get; set; } = [];

    [JsonPropertyName("programInvocations")]
    public List<ProgramInvocation> ProgramInvocations { get; set; } = [];

    public string Signature { get; set; }
    public ulong Slot { get; set; }
    public bool Success { get; set; }
}