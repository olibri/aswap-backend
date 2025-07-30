using Microsoft.AspNetCore.Mvc;
using Solnet.Wallet.Utilities;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Chaos.NaCl;
using Shouldly;

namespace Tests_back.Extensions.AccountAuth;

public static class WalletTestExtensions
{
  public readonly record struct TestWallet(string Address, byte[] PrivateKey);

  public static TestWallet CreateWallet()
  {
    // seed 32 bytes
    var seed = RandomNumberGenerator.GetBytes(32);

    Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey, seed);
    var address = Encoders.Base58.EncodeData(publicKey);

    return new TestWallet(address, privateKey);
  }

  public static string Sign(this TestWallet wallet, string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    var sig = Ed25519.Sign(bytes, wallet.PrivateKey);
    return Encoders.Base58.EncodeData(sig);
  }


  public static T ExtractAnonProp<T>(this OkObjectResult result, string propName)
  {
    result.Value.ShouldNotBeNull();
    var prop = result.Value!.GetType()
      .GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
    prop.ShouldNotBeNull($"Property '{propName}' not found on anonymous object.");
    var val = prop.GetValue(result.Value);
    val.ShouldNotBeNull();
    return (T)val;
  }
}