namespace App.Utils;

public static class AddressValidator
{
  public static bool IsValidSolanaAddress(string wallet)
  {
    if (string.IsNullOrWhiteSpace(wallet) || wallet.Length is < 32 or > 44)
      return false;

    try
    {
      var bytes = Solnet.Wallet.Utilities.Encoders.Base58.DecodeData(wallet);
      return bytes is { Length: 32 };
    }
    catch
    {
      return false;
    }
  }
}