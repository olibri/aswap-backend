using App.Mapper;
using Domain.Enums;
using Domain.Interfaces.CoinJelly;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Api.CoinJelly;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.CoinJelly;

public sealed class CoinJellyService(
  IDbContextFactory<P2PDbContext> dbFactory,
  ITgBotHandler tgBotHandler,
  ILogger<CoinJellyService> log)
  : ICoinJellyService
{
  public async Task<Guid> AddNewCoinJellyMethod(CoinJellyDto dto, CancellationToken ct)
  {
    Validate(dto);

    await using var db = await NewDb(ct);
    var (code, chain) = NormalizeCodes(dto);

    var existing = await FindMethodAsync(db, code, chain, ct);
    if (existing is null)
      return await InsertMethodAsync(db, dto, ct);

    await UpdateWalletIfChangedAsync(db, existing, dto.CompanyWalletAddress, ct);
    return existing.Id;
  }

  public async Task<CoinJellyAccountHistoryRequest[]> GetJellyHistoryAsync(string userWallet, CancellationToken ct)
  {
    await using var db = await NewDb(ct);

    var items = await db.Set<CoinJellyAccountHistoryEntity>()
      .Where(x => x.UserWallet == userWallet.Trim())
      .AsNoTracking()
      .OrderByDescending(x => x.CreatedAtUtc)
      .Take(200)
      .ToListAsync(ct);

    return items.Select(CoinJellyMapper.ToApi).ToArray();
  }

  public async Task<CoinJellyAccountHistoryRequest> CreateNewJellyAsync(NewUserCoinJellyRequest dto,
    CancellationToken ct)
  {
    await using var db = await NewDb(ct);

    var entity = new CoinJellyAccountHistoryEntity
    {
      Id = Guid.NewGuid(),
      TxID = " ",
      CryptoSend = dto.CryptoCurrencyFromUser,
      CryptoGet = dto.NewUserCrypto,
      AmountSend = dto.AmountSend,
      AmountGet = dto.AmountGet,
      FeeAtomic = 5,
      CreatedAtUtc = DateTime.UtcNow,
      Status = CoinJellyStatus.Initialized,
      UserWallet = dto.UserWallet,
      CryptoGetChain = dto.NewUserCryptoChain
    };

    db.Add(entity);
    await db.SaveChangesAsync(ct);
    var api = CoinJellyMapper.ToApi(entity);

    try
    {
      await tgBotHandler.NotifyAdminCoinJellyAsync(dto);
    }
    catch (Exception ex)
    {
      log.LogWarning(ex, "[TG] Admin notify failed for CoinJelly TxID={TxID}", entity.TxID);
    }

    return api;
  }

  public async Task<CoinJellyAccountHistoryRequest> UpdateJellyAsync(CoinJellyUpdateRequest req, CancellationToken ct)
  {
    await using var db = await NewDb(ct);

    var e = await db.Set<CoinJellyAccountHistoryEntity>()
              .FirstOrDefaultAsync(x => x.Id == req.Id, ct)
            ?? throw new KeyNotFoundException($"CoinJelly history not found: {req.Id}");

    var changed = false;

    var newTx = req.TxID.Trim();
    if (!string.IsNullOrWhiteSpace(newTx) && e.TxID != newTx)
    {
      e.TxID = newTx!;
      changed = true;
    }

    if (e.Status != req.Status)
    {
      e.Status = req.Status;
      changed = true;
    }

    if (changed)
      await db.SaveChangesAsync(ct);

    return CoinJellyMapper.ToApi(e);
  }

  public async Task<CoinJellyDto[]> GetAllJellyMethodsAsync(CancellationToken ct)
  {
    await using var db = await NewDb(ct);

    var rows = await db.Set<CoinJellyEntity>()
      .AsNoTracking()
      .OrderBy(x => x.CryptoCurrency)
      .ThenBy(x => x.CryptoCurrencyChain)
      .ToListAsync(ct);

    return rows
      .Select(e => new CoinJellyDto(
        e.CompanyWalletAddress,
        e.CryptoCurrency,
        e.CryptoCurrencyChain))
      .ToArray();
  }


  private static void Validate(CoinJellyDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto.CryptoCurrency)) throw new ArgumentException("CryptoCurrency is required.");
    if (string.IsNullOrWhiteSpace(dto.CryptoCurrencyChain))
      throw new ArgumentException("CryptoCurrencyChain is required.");
    if (string.IsNullOrWhiteSpace(dto.CompanyWalletAddress))
      throw new ArgumentException("CompanyWalletAddress is required.");
  }

  private static (string Code, string Chain) NormalizeCodes(CoinJellyDto dto)
  {
    return (dto.CryptoCurrency.Trim().ToUpperInvariant(), dto.CryptoCurrencyChain.Trim().ToUpperInvariant());
  }


  private async Task<P2PDbContext> NewDb(CancellationToken ct)
  {
    return await dbFactory.CreateDbContextAsync(ct);
  }

  private static Task<CoinJellyEntity?> FindMethodAsync(P2PDbContext db, string code, string chain,
    CancellationToken ct)
  {
    return db.Set<CoinJellyEntity>()
      .FirstOrDefaultAsync(x => x.CryptoCurrency == code && x.CryptoCurrencyChain == chain, ct);
  }

  private static async Task<Guid> InsertMethodAsync(P2PDbContext db, CoinJellyDto dto, CancellationToken ct)
  {
    var entity = CoinJellyMapper.ToEntity(dto);
    db.Add(entity);
    await db.SaveChangesAsync(ct);
    return entity.Id;
  }

  private static async Task UpdateWalletIfChangedAsync(P2PDbContext db, CoinJellyEntity e, string newWallet,
    CancellationToken ct)
  {
    var trimmed = newWallet.Trim();
    if (e.CompanyWalletAddress == trimmed) return;

    e.CompanyWalletAddress = trimmed;
    await db.SaveChangesAsync(ct);
  }
}