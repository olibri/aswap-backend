namespace Domain.Interfaces.Services.Auth;

public interface ISignatureVerifier
{
  bool Verify(string network, string wallet, string message, string signature);
}