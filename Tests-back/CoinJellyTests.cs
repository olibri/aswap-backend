using Domain.Enums;
using Domain.Interfaces.CoinJelly;
using Domain.Models.Api.CoinJelly;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.CoinJelly;

namespace Tests_back;

public class CoinJellyTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task Add_New_Coin_JellyMethod()
  {
    PostgresDatabase.ResetState("coin_jelly");

    var coinJelly = fixture.GetService<ICoinJellyService>();

    var id1 = await coinJelly.AddMethodAsync("WALLET1", "USDT", "SOLANA");
    var id2 = await coinJelly.AddMethodAsync("WALLET2", "usdt", "solana");

    id1.ShouldNotBe(Guid.Empty);
    id2.ShouldNotBe(Guid.Empty);


    var factory = fixture.GetService<IDbContextFactory<P2PDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    var e = await db.Set<CoinJellyEntity>().FirstAsync(x => x.Id == id1);
    e.CompanyWalletAddress.ShouldBe("WALLET2");
  }

  [Fact(Skip = "Tg call non deterministic")]
  public async Task History_contains_created_items()
  {
    PostgresDatabase.ResetState("coin_jelly_account_history_entity");
    PostgresDatabase.ResetState("coin_jelly");

    var svc = fixture.GetService<ICoinJellyService>();

    var a = await svc.CreateRandomJellyAsync();
    var b = await svc.CreateRandomJellyAsync();

    var history = await svc.GetJellyHistoryAsync(a.UserWallet, default);

    history.Select(h => h.CryptoSend).ShouldNotBeEmpty();
  }

  [Fact(Skip = "Tg call non deterministic")]
  public async Task Create_normalizes_codes()
  {
    PostgresDatabase.ResetState("coin_jelly_account_history_entity");
    PostgresDatabase.ResetState("coin_jelly");

    var svc = fixture.GetService<ICoinJellyService>();

    var x = await svc.CreateJellyAsync("W1", "usdt", "solana");

    x.CryptoSend.ShouldBe("usdt");
    x.CryptoGet.ShouldBe("usdt");
  }

  [Fact(Skip = "Tg call non deterministic")]
  public async Task UpdateJelly_updates_txid_and_status()
  {
    // Arrange
    PostgresDatabase.ResetState("coin_jelly_account_history_entity");
    var svc = fixture.GetService<ICoinJellyService>();
    var created = await svc.CreateJellyAsync("W_TEST", "usdt", "solana");

    var factory = fixture.GetService<IDbContextFactory<P2PDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    var id = await db.Set<CoinJellyAccountHistoryEntity>()
      .Where(x => x.TxID == created.TxID)
      .Select(x => x.Id)
      .FirstAsync();

    var newTx = "tx_" + Guid.NewGuid().ToString("N")[..10];
    var req = new CoinJellyUpdateRequest(id, newTx, CoinJellyStatus.Completed);

    // Act
    var updated = await svc.UpdateJellyAsync(req, default);

    // Assert (DTO)
    updated.TxID.ShouldBe(newTx);
    updated.Status.ShouldBe(CoinJellyStatus.Completed);

    // Assert (DB)
    var row = await db.Set<CoinJellyAccountHistoryEntity>().FirstAsync(x => x.Id == id);
    row.TxID.ShouldBe(newTx);
    row.Status.ShouldBe(CoinJellyStatus.Completed);
  }
}