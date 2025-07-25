namespace Domain.Interfaces.Services.Auth;

public interface INetworkVerifier
{
  string Network { get; }
  bool Verify(string wallet, string message, string signature);
}