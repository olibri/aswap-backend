namespace Domain.Models.Api.Auth;

public record SaveRefreshDto(
  string Wallet,
  string RefreshToken,
  DateTime ExpiresUtc,
  string? UserAgent = null,
  string? Ip = null);