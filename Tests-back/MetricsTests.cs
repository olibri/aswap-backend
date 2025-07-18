using App.Metrics;
using Domain.Enums;
using Domain.Interfaces.Metrics;
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
  public async Task Tvl_Is_Saved_To_Snapshot()
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await db.SeedAsync(
      count: 50,
      status: EscrowStatus.OnChain,
      tokenMint: "USDc",
      side: OrderSide.Sell);

    var tvlTask = scope.ServiceProvider             
      .GetServices<IPeriodicTask>()
      .OfType<TvlSnapshotTask>()
      .Single();


    await tvlTask.ExecuteAsync(CancellationToken.None);

    var snap = await db.TvlSnapshots
      .SingleAsync(s => s.TokenMint == "USDc");

    snap.Balance.ShouldBe(50 * 100);
  }
}