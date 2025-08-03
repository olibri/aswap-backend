namespace Domain.Models.Api.Auth;

public sealed record TokenPair(
  string AccessToken,
  string RefreshToken,
  DateTime JwtExpiresAt,
  DateTime RefreshExpiresAt);