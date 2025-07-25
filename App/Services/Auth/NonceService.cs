using Domain.Interfaces.Services.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace App.Services.Auth;

public sealed class NonceService(IMemoryCache cache) : INonceService
{
  private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

  public string Issue(string wallet)
  {
    var nonce = Guid.NewGuid().ToString("N");
    cache.Set(GetKey(wallet), nonce, Lifetime);
    return nonce;
  }

  public Task<bool> ValidateAsync(string wallet, string nonce, CancellationToken _)
  {
    if (!cache.TryGetValue(GetKey(wallet), out string? stored) || stored != nonce)
      return Task.FromResult(false);

    cache.Remove(GetKey(wallet));
    return Task.FromResult(true);
  }

  private static string GetKey(string wallet) => $"nonce:{wallet}";
}