using Domain.Interfaces.CoinJelly;
using Domain.Models.Api.CoinJelly;
using System.Security.Cryptography;

namespace Tests_back.Extensions.CoinJelly;

public static class CoinJellyTestExtensions
{
  public static Task<Guid> AddMethodAsync(
    this ICoinJellyService svc,
    string wallet = "  WALLET_TEST  ",
    string code = "usdt",
    string chain = "solana",
    CancellationToken ct = default)
    => svc.AddNewCoinJellyMethod(new CoinJellyDto(new Guid(), wallet, code, chain), ct);

    public static Task<CoinJellyAccountHistoryRequest> CreateJellyAsync(
      this ICoinJellyService svc,
      string userWallet,
      string code,
      string chain,
      decimal amountSend = 100,
      decimal amountGet = 100,
      CancellationToken ct = default)
    {
      var req = new NewUserCoinJellyRequest(
        CryptoCurrencyFromUser: code,
        UserWallet: userWallet,
        NewUserWallet: $"NEW_{userWallet}",
        NewUserCrypto: code,
        NewUserCryptoChain: chain,
        AmountSend: amountSend,
        AmountGet: amountGet
      );
      return svc.CreateNewJellyAsync(req, ct);
    }

    public static Task<CoinJellyAccountHistoryRequest> CreateRandomJellyAsync(
      this ICoinJellyService svc,
      CancellationToken ct = default)
    {
      var codes = new[] { "usdt", "btc", "eth", "sol" };
      var chains = new[] { "solana", "ethereum", "bsc" };

      string code = codes[RandIndex(codes.Length)];
      string chain = chains[RandIndex(chains.Length)];
      string user = $"W_{RandHex(6)}";

      var req = new NewUserCoinJellyRequest(
        CryptoCurrencyFromUser: code,
        UserWallet: user,
        NewUserWallet: $"NEW_{user}",
        NewUserCrypto: code,             
        NewUserCryptoChain: chain,
        AmountSend: RandAmount(),
        AmountGet: RandAmount()
      );
      return svc.CreateNewJellyAsync(req, ct);
    }

    private static int RandIndex(int maxExclusive)
    {
      Span<byte> b = stackalloc byte[4];
      RandomNumberGenerator.Fill(b);
      var v = BitConverter.ToUInt32(b);
      return (int)(v % (uint)maxExclusive);
    }

    private static string RandHex(int bytes)
    {
      var arr = new byte[bytes];
      RandomNumberGenerator.Fill(arr);
      return Convert.ToHexString(arr);
    }

    private static decimal RandAmount()
    {
      Span<byte> b = stackalloc byte[4];
      RandomNumberGenerator.Fill(b);
      var v = BitConverter.ToUInt32(b) % 1000u; // 0..999
      return (decimal)v + 1;                    // 1..1000
    }

  }