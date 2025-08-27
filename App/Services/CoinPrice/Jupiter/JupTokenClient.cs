using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Models.Api.CoinPrice;
using System.Text.Json;

namespace App.Services.CoinPrice.Jupiter;

public sealed class JupTokenClient(HttpClient http) : IJupTokenClient
{
  public async Task<IReadOnlyList<TokenDto>> GetVerifiedTokensAsync(CancellationToken ct)
  {
    using var res = await http.GetAsync("/tokens/v2/tag?query=verified", ct);
    res.EnsureSuccessStatusCode();
      
    await using var stream = await res.Content.ReadAsStreamAsync(ct);
    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

    IEnumerable<JsonElement> items;

    var root = doc.RootElement;
    if (root.ValueKind == JsonValueKind.Array)
    {
      items = root.EnumerateArray();
    }
    else if (root.ValueKind == JsonValueKind.Object
             && root.TryGetProperty("data", out var data)
             && data.ValueKind == JsonValueKind.Array)
    {
      items = data.EnumerateArray();
    }
    else
    {
      return Array.Empty<TokenDto>();
    }


    var list = new List<TokenDto>();
    foreach (var el in items)
    {
      var mint = el.TryGetProperty("address", out var a) ? a.GetString()
        : el.TryGetProperty("id", out var i) ? i.GetString()
        : null;
      if (string.IsNullOrWhiteSpace(mint)) continue;

      var symbol = el.TryGetProperty("symbol", out var s) ? s.GetString() : null;
      var name = el.TryGetProperty("name", out var n) ? n.GetString() : null;
      int? decimals = el.TryGetProperty("decimals", out var d) && d.TryGetInt32(out var di) ? di : null;
      var iconUrl = el.TryGetProperty("icon", out var icon) ? icon.GetString() : null;

      list.Add(new TokenDto(mint, symbol, name, decimals, true, iconUrl));
    }

    return list;
  }
}