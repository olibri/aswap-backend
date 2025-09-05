using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Models.Api.CoinPrice;
using Domain.Models.Dtos.Jupiter;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace App.Services.CoinPrice.Jupiter;

public sealed class JupSwap(HttpClient http) : IJupiterSwapApi
{
  private static readonly JsonSerializerOptions Json = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };

  public async Task<QuoteResponseDto> GetQuoteAsync(QuoteRequest req, CancellationToken ct = default)
  {
    var qs = new StringBuilder()
      .Append("?inputMint=").Append(Uri.EscapeDataString(req.InputMint))
      .Append("&outputMint=").Append(Uri.EscapeDataString(req.OutputMint))
      .Append("&amount=").Append(req.Amount.ToString())
      .Append("&swapMode=").Append(req.SwapMode.ToString())
      .Append("&slippageBps=").Append(req.SlippageBps.ToString())
      .ToString();

    using var res = await http.GetAsync($"quote{qs}", ct);
    res.EnsureSuccessStatusCode();

    await using var stream = await res.Content.ReadAsStreamAsync(ct);
    var dto = await JsonSerializer.DeserializeAsync<QuoteResponseDto>(stream, Json, ct)
              ?? throw new InvalidOperationException("Empty quote response");

    return dto;
  }

  public async Task<SwapResponseDto> CreateSwapAsync(string userPublicKey, QuoteResponseDto quote,
    SwapOptions? opts = null, CancellationToken ct = default)
  {
    if (string.IsNullOrWhiteSpace(userPublicKey))
      throw new ArgumentException("userPublicKey is required", nameof(userPublicKey));

    var payload = new
    {
      userPublicKey,
      quoteResponse = quote,
      prioritizationFeeLamports = opts?.PrioritizationFeeLamports,
      priorityLevelWithMaxLamports = opts?.PriorityLevelWithMaxLamports is null
        ? null
        : new
        {
          maxLamports = opts.PriorityLevelWithMaxLamports.MaxLamports,
          priorityLevel = opts.PriorityLevelWithMaxLamports.PriorityLevel
        },
      dynamicComputeUnitLimit = opts?.DynamicComputeUnitLimit ?? true
    };

    using var content = new StringContent(JsonSerializer.Serialize(payload, Json), Encoding.UTF8, "application/json");
    using var res = await http.PostAsync("swap", content, ct);
    res.EnsureSuccessStatusCode();

    await using var stream = await res.Content.ReadAsStreamAsync(ct);
    var dto = await JsonSerializer.DeserializeAsync<SwapResponseDto>(stream, Json, ct)
              ?? throw new InvalidOperationException("Empty swap response");

    return dto;
  }
}