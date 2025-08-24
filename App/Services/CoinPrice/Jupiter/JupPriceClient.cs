using System.Text.Json;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Models.Api.CoinPrice;

namespace App.Services.CoinPrice.Jupiter;

public sealed class JupPriceClient(HttpClient http) : IJupPriceClient
{
  public async Task<IReadOnlyDictionary<string, JupPriceItemDto>> GetUsdPricesAsync(
    IEnumerable<string> mints, CancellationToken ct)
  {
    var ids = string.Join(",", mints);
    var url = $"/price/v3/price?ids={Uri.EscapeDataString(ids)}&vsToken=USDC";

    using var res = await http.GetAsync(url, ct);
    res.EnsureSuccessStatusCode();

    await using var stream = await res.Content.ReadAsStreamAsync(ct);
    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

    var root = doc.RootElement;
    var data = root.TryGetProperty("data", out var d) ? d : root;

    var map = new Dictionary<string, JupPriceItemDto>(StringComparer.Ordinal);
    foreach (var kv in data.EnumerateObject())
    {
      var mint = kv.Name;
      var obj = kv.Value;

      var usd = GetDecimal(obj, "usdPrice") ?? GetDecimal(obj, "price") ?? 0m;
      if (usd <= 0) continue;

      var blockId = obj.TryGetProperty("blockId", out var b) && b.TryGetInt64(out var bi) ? bi : 0;
      var decimals = obj.TryGetProperty("decimals", out var de) && de.TryGetInt32(out var di) ? di : 0;
      var change24h = GetDecimal(obj, "priceChange24h") ?? 0m;

      map[mint] = new JupPriceItemDto(usd, blockId, decimals, change24h);
    }

    return map;

    static decimal? GetDecimal(JsonElement el, string name)
    {
      return el.TryGetProperty(name, out var p) 
             && p.ValueKind is JsonValueKind.Number && p.TryGetDecimal(out var v)
        ? v
        : null;
    }
  }
}