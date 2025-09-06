using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.Services.CoinService.TokenRepo;
using Domain.Models.DB;
using Domain.Models.Dtos.Jupiter;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using App.Mapper;
using Domain.Models.Dtos;

namespace App.Services.CoinPrice.Swap;

public sealed class SwapService(
  IJupiterSwapApi jupiter,
  ITokenRepository tokenRepository,
  ICoinService coinService,
  IDbContextFactory<P2PDbContext> dbFactory) : ISwapService
{
  public async Task<SwapResponseDto> AddSwapAsync(SwapBuildRequestDto body, CancellationToken ct)
  {
    var res = await jupiter.CreateSwapAsync(
      body.UserPublicKey,
      body.Quote,
      body.Options,
      ct);

    await SaveHistoryAsync(body, res, ct);

    return res;
  }

  public async Task<AccountSwapHistoryDto> SwapHistoryAsync(string userWallet, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(userWallet))
      throw new ArgumentException("Value cannot be null or whitespace.", nameof(userWallet));

    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var entity = await db.AccountSwapHistory
      .AsNoTracking()
      .Where(x => x.UserWallet == userWallet)
      .OrderByDescending(x => x.CreatedAtUtc)
      .FirstOrDefaultAsync(ct);

    if (entity is null)
      throw new InvalidOperationException($"No swap history found for wallet '{userWallet}'.");

    return entity.ToDto();
  }

  private async Task SaveHistoryAsync(
    SwapBuildRequestDto request,
    SwapResponseDto swapResult,
    CancellationToken ct)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var fromToken = await tokenRepository.GetByMintAsync(
      request.Quote.InputMint, ct);
    var toToken = await tokenRepository.GetByMintAsync(
      request.Quote.OutputMint, ct);

    var amountIn = decimal.Parse(
      request.Quote.InAmount,
      NumberStyles.Any,
      CultureInfo.InvariantCulture);
    var amountOut = decimal.Parse(
      request.Quote.OutAmount,
      NumberStyles.Any,
      CultureInfo.InvariantCulture);

    var (priceUsdInToken, priceUsdOutToken) = await coinService
      .GetLastPriceAsync(fromToken.Mint, toToken.Mint);

    var history = new AccountSwapHistoryEntity
    {
      Tx = swapResult.SwapTransaction,
      UserWallet = request.UserPublicKey,
      CryptoFrom = fromToken.Symbol,
      CryptoTo = toToken.Symbol,
      AmountIn = amountIn,
      AmountOut = amountOut,
      PriceUsdIn = amountIn * priceUsdInToken,
      PriceUsdOut = amountOut * priceUsdOutToken,
      CreatedAtUtc = DateTime.UtcNow
    };

    db.Set<AccountSwapHistoryEntity>().Add(history);
    await db.SaveChangesAsync(ct);
  }
}