namespace Domain.Interfaces.Services.Auth;

public interface IRefreshTokenService
{
  Task SaveAsync(string wallet, string refresh, string? ua, string? ip, DateTime expiresUtc, CancellationToken ct);
  Task<(long Id, string Wallet)?> ValidateAsync(string refresh, CancellationToken ct);

  Task RotateAsync(long id, string oldRefresh, string newRefresh, DateTime newExpiresUtc, string? ua, string? ip,
    CancellationToken ct);

  Task RevokeAsync(long id, CancellationToken ct);
}