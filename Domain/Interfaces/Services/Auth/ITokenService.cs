using Domain.Models.Api.Auth;

namespace Domain.Interfaces.Services.Auth;

public interface ITokenService
{
  TokenPair Generate(string wallet, string role, DateTime? banUntil);
}