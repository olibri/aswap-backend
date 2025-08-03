using System.Net;
using Domain.Interfaces.Services.IP;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace App.Utils.Web.IP;

public sealed class GeoIpService : IGeoIpService, IDisposable
{
  private const string DbFile = "GeoData/GeoLite2-Country.mmdb";
  private readonly IMemoryCache _cache;
  private readonly ILogger<GeoIpService> _log;
  private readonly DatabaseReader? _reader;

  public GeoIpService(IMemoryCache cache, ILogger<GeoIpService> log)
  {
    _cache = cache;
    _log = log;

    if (File.Exists(DbFile))
      _reader = new DatabaseReader(DbFile);
    else
      _log.LogWarning("GeoIP DB file '{File}' not found – Geo-lookup disabled", DbFile);
  }

  public string? ResolveCountry(string ip)
  {
    if (_reader is null)
      return null;

    if (string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip, out var addr))
      return null;

    if (_cache.TryGetValue(addr, out string? cached))
      return cached;

    try
    {
      var iso = _reader.Country(addr).Country.IsoCode;
      _cache.Set(addr, iso, TimeSpan.FromHours(24));
      return iso;
    }
    catch (AddressNotFoundException)
    {
      _cache.Set<string?>(addr, null, TimeSpan.FromHours(24));
      return null;
    }
  }

  public void Dispose()
  {
    _reader?.Dispose();
  }
}