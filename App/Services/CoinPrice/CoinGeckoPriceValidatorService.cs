using Domain.Interfaces.Services.CoinService;
using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace App.Services.CoinPrice;

public class CoinGeckoPriceValidatorService : IPriceValidatorService
{
  private readonly HttpClient _http;

  private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

  private record struct PriceCache(decimal Usd, DateTimeOffset Ts);

  private static readonly ConcurrentDictionary<string, PriceCache> Cache = new();
  private static readonly ConcurrentDictionary<string, Lazy<Task<decimal?>>> Inflight = new();

  private static readonly IReadOnlyDictionary<string, string> IdMap =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
      ["USDT"] = "tether",
      ["USDC"] = "usd-coin",
      ["BTC"] = "bitcoin",
      ["ETH"] = "ethereum",
      ["BNB"] = "binancecoin",
      ["SOL"] = "solana",
      ["TRX"] = "tron"
    };

  public CoinGeckoPriceValidatorService(HttpClient? http = null)
  {
    _http = http ?? new HttpClient
    {
      Timeout = TimeSpan.FromSeconds(5)
    };
    _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
  }

  public async Task<decimal?> GetUsdPriceAsync(string symbol, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(symbol)) return null;

    var id = IdMap.GetValueOrDefault(symbol.Trim());
    if (id is null) return null;

    var now = DateTimeOffset.UtcNow;

    if (Cache.TryGetValue(id, out var cached) && now - cached.Ts < Ttl)
      return cached.Usd;

    var lazy = Inflight.GetOrAdd(id, _ => new Lazy<Task<decimal?>>(async () =>
    {
      try
      {
        var url = $"https://api.coingecko.com/api/v3/simple/price?ids={Uri.EscapeDataString(id)}&vs_currencies=usd";
        using var res = await _http.GetAsync(url, ct);
        if (!res.IsSuccessStatusCode) return (decimal?)null;

        var json =
          await res.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>(cancellationToken: ct);
        if (json is null || !json.TryGetValue(id, out var row) || !row.TryGetValue("usd", out var usd) || usd <= 0)
          return (decimal?)null;

        Cache[id] = new PriceCache(usd, DateTimeOffset.UtcNow);
        return usd;
      }
      finally
      {
        Inflight.TryRemove(id, out var removed);
      }
    }));

    return await lazy.Value;
  }
}