namespace Domain.Interfaces.Services.Auth;

public interface INonceService
{
  Task<bool> ValidateAsync(string wallet, string nonce, CancellationToken ct);
  string Issue(string wallet);
}