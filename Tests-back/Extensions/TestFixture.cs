using Aswap_back.Configuration;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests_back.Extensions;

public class TestFixture : IDisposable
{
  public readonly IHost Host = RootBuilder.GetHost();

  public TestFixture()
  {
    Host.Start();
  }

  public T GetService<T>()
  {
    return Host.Services.GetService<T>()!;
  }

  public void Dispose()
  {
    Host.StopAsync().GetAwaiter().GetResult();
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("tvl_snapshots");
    PostgresDatabase.ResetState("account");
    PostgresDatabase.ResetState("rooms");
    PostgresDatabase.ResetState("messages");
    PostgresDatabase.ResetState("telegram_link");
    PostgresDatabase.ResetState("outbox_messages");
    PostgresDatabase.ResetState("events");
    PostgresDatabase.ResetState("order_created_daily");
    PostgresDatabase.ResetState("order_status_daily");
    PostgresDatabase.ResetState("asset_volume_daily");
    PostgresDatabase.ResetState("deal_time_daily");
    PostgresDatabase.ResetState("user_metrics_daily");
    PostgresDatabase.ResetState("rating_reviews");


    Host.Dispose();
  }
}

public static class TestFixtureExtensions
{
  public static void ResetDb(this TestFixture f, params string[] tables)
  {
    foreach (var table in tables)
    {
      PostgresDatabase.ResetState(table);
    }
    var db = f.GetService<P2PDbContext>();
    db.ChangeTracker.Clear();
  }
}
