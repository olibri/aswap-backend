using App.Mapper;
using App.Services.QuerySpec.Realization;
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
  private const decimal Persentage = 3; 
  public async Task<Guid> AddNewCoinJellyMethod(CoinJellyDto dto, CancellationToken ct)
  {
    Validate(dto);

    await using var db = await NewDb(ct);

    var existing = await FindMethodAsync(db, dto.CryptoCurrencyCode, dto.CryptoCurrencyChain, ct);
    if (existing is null)
      return await InsertMethodAsync(db, dto, ct);

    await UpdateWalletIfChangedAsync(db, existing, dto.CompanyWalletAddress, ct);
    return existing.Id;
  }

  public async Task<bool> DeleteCoinJellyMethod(Guid id, CancellationToken ct)
  {
    await using var db = await NewDb(ct);
    var entity = await db.Set<CoinJellyEntity>()
                   .FirstOrDefaultAsync(x => x.Id == id, ct)
                 ?? throw new KeyNotFoundException($"CoinJelly method not found: {id}");
    db.Remove(entity);
    await db.SaveChangesAsync(ct);
    return true;
  }

  public async Task<CoinJellyAccountHistoryRequest[]> GetUserJellyHistoryAsync(string userWallet, CancellationToken ct)
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
    //TODO: add price validation
    //if (dto.AmountUserWannaGet > dto.AmountUserSend * (Persentage / 100))
    //{
    //  // Return null to indicate a bad request, or throw an exception if you want to handle it elsewhere.
    //  // Alternatively, you can change the return type to IResult and return Results.BadRequest directly.
    //  return null;
    //}
    await using var db = await NewDb(ct);

    var entity = new CoinJellyAccountHistoryEntity
    {
      Id = Guid.NewGuid(),
      TxID = " ",
      CryptoSend = dto.CryptoCurrencyFromUser,
      CryptoGet = dto.NewUserCrypto,
      AmountSend = dto.AmountUserSend,
      AmountGet = dto.AmountUserWannaGet,
      FeeAtomic = 3,
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
      .OrderBy(x => x.CryptoCurrencyCode)
      .ThenBy(x => x.CryptoCurrencyChain)
      .ToListAsync(ct);

    return rows
      .Select(e => new CoinJellyDto(
        e.Id,
        e.CompanyWalletAddress,
        e.CryptoCurrencyCode,
        e.CryptoCurrencyName,
        e.CryptoCurrencyChain))
      .ToArray();
  }

  public async Task<CoinJellyAccountHistoryRequest[]> GetAllJellyHistoryAsync(CoinJellyHistoryQueryAsync q,CancellationToken ct)
  {
    await using var db = await NewDb(ct);
    var src = db.Set<CoinJellyAccountHistoryEntity>().AsNoTracking();

    var page = await q.BuildSpec().ExecuteAsync(src);
    return page.Data.Select(CoinJellyMapper.ToApi).ToArray();
  }

  private static void Validate(CoinJellyDto dto)
  {
    if (string.IsNullOrWhiteSpace(dto.CryptoCurrencyCode)) throw new ArgumentException("CryptoCurrencyCode is required.");
    if (string.IsNullOrWhiteSpace(dto.CryptoCurrencyName)) throw new ArgumentException("CryptoCurrencyName is required.");
    if (string.IsNullOrWhiteSpace(dto.CryptoCurrencyChain))
      throw new ArgumentException("CryptoCurrencyChain is required.");
    if (string.IsNullOrWhiteSpace(dto.CompanyWalletAddress))
      throw new ArgumentException("CompanyWalletAddress is required.");
  }


  private async Task<P2PDbContext> NewDb(CancellationToken ct)
  {
    return await dbFactory.CreateDbContextAsync(ct);
  }

  private static Task<CoinJellyEntity?> FindMethodAsync(P2PDbContext db, string code, string chain,
    CancellationToken ct)
  {
    return db.Set<CoinJellyEntity>()
      .FirstOrDefaultAsync(x => x.CryptoCurrencyCode == code && x.CryptoCurrencyChain == chain, ct);
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