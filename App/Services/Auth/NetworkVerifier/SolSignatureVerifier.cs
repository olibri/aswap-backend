using Domain.Interfaces.Services.Auth;
using Solnet.Wallet.Utilities;

namespace App.Services.Auth.NetworkVerifier;

public sealed class SolSignatureVerifier : INetworkVerifier
{
  public string Network => "sol";

  public bool Verify(string wallet, string msg, string sig)
  {
    var pubKey = Solnet.Wallet.Utilities.Encoders.Base58.DecodeData(wallet);
    var sign = Encoders.Base58.DecodeData(sig);
    var bytes = System.Text.Encoding.UTF8.GetBytes(msg);
    return Chaos.NaCl.Ed25519.Verify(sign, bytes, pubKey);
  }
}