using Chaos.NaCl;
using Domain.Interfaces.Services.Auth;
using Solnet.Wallet.Utilities;

namespace App.Services.Auth;

public sealed class SignatureVerifier(IEnumerable<INetworkVerifier> verifiers)
  : ISignatureVerifier
{
  private readonly Dictionary<string, INetworkVerifier> _map =
    verifiers.ToDictionary(v => v.Network, StringComparer.OrdinalIgnoreCase);

  public bool Verify(string network, string wallet, string message, string signature)
  {
    return _map.TryGetValue(network, out var v) && v.Verify(wallet, message, signature);
  }
}