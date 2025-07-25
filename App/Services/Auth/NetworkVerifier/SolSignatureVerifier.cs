using Domain.Interfaces.Services.Auth;

namespace App.Services.Auth.NetworkVerifier;

public sealed class SolSignatureVerifier : INetworkVerifier
{
  public string Network => "sol";

  public bool Verify(string wallet, string msg, string sig)
  {
    var pubKey = Solnet.Wallet.Utilities.Encoders.Base58.DecodeData(wallet);
    var sign = Convert.FromBase64String(sig);
    var bytes = System.Text.Encoding.UTF8.GetBytes(msg);
    return Chaos.NaCl.Ed25519.Verify(sign, bytes, pubKey);
  }
}