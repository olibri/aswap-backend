using App.Metrics;
using App.Utils;
using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class MetricsTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public void TvlUtils_Calculates_Correctly()
  {
    var expectedTvl = 130m;
    var orders = new[]
    {
      new EscrowOrderEntity { TokenMint = "USDc", Status = EscrowStatus.OnChain, Amount = 100, FilledQuantity = 0 },
      new EscrowOrderEntity
        { TokenMint = "USDc", Status = EscrowStatus.PartiallyOnChain, Amount = 50, FilledQuantity = 20 },
      new EscrowOrderEntity { TokenMint = "BONK", Status = EscrowStatus.Cancelled, Amount = 99 }
    };

    var tvl = TvlUtils.Calculate(orders);

    tvl["USDc"].ShouldBe(expectedTvl);
    tvl.ContainsKey("BONK").ShouldBeFalse();
  }

  [Fact]
  public async Task Tvl_Is_Saved_To_Snapshot()
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();


    var seeded = await db.SeedAsync(
      50,
      EscrowStatus.OnChain,
      "USDc",
      OrderSide.Sell,
      fixedAmount: 100);

    var factory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
    var task = new TvlSnapshotTask(factory);

    await task.ExecuteAsync(CancellationToken.None);

    var snap = await db.TvlSnapshots
      .Where(s => s.TokenMint == "USDc")
      .OrderByDescending(s => s.TakenAt)
      .FirstAsync();

    var expected = seeded.ExpectedTvl("USDc");
    snap.Balance.ShouldBe(expected);
  }
}