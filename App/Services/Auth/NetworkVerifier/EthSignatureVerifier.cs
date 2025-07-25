using Domain.Interfaces.Services.Auth;

namespace App.Services.Auth.NetworkVerifier;

public sealed class EthSignatureVerifier : INetworkVerifier
{
  public string Network => "eth";

  public bool Verify(string wallet, string msg, string sig)
  {
    var signer = new Nethereum.Signer.EthereumMessageSigner();
    var recovered = signer.EncodeUTF8AndEcRecover(msg, sig);  
    return string.Equals(recovered, wallet, StringComparison.OrdinalIgnoreCase);
  }
}