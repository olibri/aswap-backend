namespace Domain.Models.Api.Auth;

public sealed class TokenOptions
{
  public string Issuer { get; init; } = default!;
  public string Audience { get; init; } = default!;
  public string SymmetricKey { get; init; } = default!; 
  public int AccessLifetimeMinutes { get; init; } = 15;
  public int RefreshLifetimeDays { get; init; } = 30;
}