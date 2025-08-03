using Domain.Models.Api.Auth;

namespace Domain.Interfaces.Services.Auth;

public interface IRefreshTokenService
{
  Task SaveAsync(SaveRefreshDto dto, CancellationToken ct);
  Task<(long Id, string Wallet)?> ValidateAsync(string refresh, CancellationToken ct);

  Task RotateAsync(RotateRefreshDto dto, CancellationToken ct);
  Task RevokeAsync(long sessionId, CancellationToken ct);
}