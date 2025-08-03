namespace Domain.Models.Api.Auth;

public record RotateRefreshDto(
  long SessionId,
  string OldRefreshToken,
  string NewRefreshToken,
  DateTime NewExpiresUtc,
  string? UserAgent = null,
  string? Ip = null);