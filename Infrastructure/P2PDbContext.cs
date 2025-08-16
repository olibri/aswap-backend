using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.DB.Currency;
using Domain.Models.DB.Metrics;
using Domain.Models.DB.PaymentMethod;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class P2PDbContext(DbContextOptions<P2PDbContext> opt) : DbContext(opt)
{
  /* ─────────── Core tables ─────────── */
  public DbSet<EscrowOrderEntity> EscrowOrders { get; set; }
  public DbSet<EscrowOrderPaymentMethodEntity> EscrowOrderPaymentMethod { get; set; }
  public DbSet<RoomEntity> Rooms { get; set; }
  public DbSet<MessageEntity> Messages { get; set; }
  public DbSet<AccountEntity> Account { get; set; }
  public DbSet<TelegramLinkEntity> TelegramLinks { get; set; }

  /* ─────────── Metrics tables ─────────── */
  public DbSet<EventEntity> Events { get; set; }
  public DbSet<OutboxMessage> OutboxMessages { get; set; }

  public DbSet<TvlSnapshotEntity> TvlSnapshots { get; set; }
  public DbSet<AssetVolumeDailyEntity> AssetVolumeDaily { get; set; }
  public DbSet<DealTimeDailyEntity> DealTimeDailyEntity { get; set; }

  public DbSet<OrderStatusDailyEntity> OrderStatusDaily { get; set; }
  public DbSet<UserMetricsDailyEntity> UserMetricsDaily { get; set; }
  public DbSet<SessionEntity> Sessions { get; set; }
  public DbSet<FunnelMetricsDailyEntity> FunnelMetricsDaily { get; set; }
  public DbSet<RefreshTokenEntity> RefreshToken { get; set; }
  public DbSet<RatingReviewEntity> Ratings { get; set; }
  public DbSet<TxHistoryEntity> TxHistory { get; set; }
  public DbSet<BanEntity> Bans { get; set; }
  public DbSet<AggregatorState> AggregatorStates { get; set; }
  public DbSet<OrderCreatedDailyEntity> OrderCreatedDaily { get; set; }

  //Payment methods and categories
  public DbSet<PaymentCategory> PaymentCategories { get; set; }
  public DbSet<PaymentMethod> PaymentMethods { get; set; }
  public DbSet<PaymentPopularityDaily> PaymentPopularityDaily { get; set; }

  public DbSet<Currency> Currencies { get; set; }

  /* ─────────── CoinJelly ─────────── */
  public DbSet<CoinJellyEntity> CoinJelly { get; set; }
  public DbSet<CoinJellyAccountHistoryEntity> CoinJellyAccountHistory { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<EscrowOrderEntity>(entity =>
    {
      entity.Property(e => e.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
      entity.Property(x => x.EscrowStatus)
        .HasConversion<int>()          // або .HasConversion<short>() якщо хочеш smallint
        .HasColumnType("integer")      // "smallint" якщо <short>
        .HasColumnName("escrow_status");
      entity.HasIndex(x => new { x.TokenMint, x.FiatCode, x.OfferSide, Status = x.EscrowStatus, x.Price })
        .HasDatabaseName("ix_escrow_best_price");
    });



    modelBuilder.Entity<EscrowOrderPaymentMethodEntity>(entity =>
    {
      entity.HasKey(x => new { x.OrderId, x.MethodId });
      entity.HasIndex(x => new { x.MethodId, x.OrderId });

      entity.HasOne(x => x.Order)
        .WithMany(o => o.PaymentMethods)
        .HasForeignKey(x => x.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(x => x.Method)
        .WithMany()
        .HasForeignKey(x => x.MethodId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<CoinJellyEntity>(e =>
    {
      e.HasKey(x => x.Id);

      e.HasIndex(x => new { x.CryptoCurrency, x.CryptoCurrencyChain })
        .IsUnique()
        .HasDatabaseName("ux_coin_jelly_currency_chain");
    });

    modelBuilder.Entity<CoinJellyAccountHistoryEntity>(e =>
    {
      e.HasKey(x => x.Id);

      e.Property(x => x.AmountSend).HasColumnType("numeric(20,0)");
      e.Property(x => x.AmountGet).HasColumnType("numeric(20,0)");
      e.Property(x => x.FeeAtomic).HasColumnType("numeric(78,0)");

      e.Property(x => x.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      e.Property(x => x.Status)
        .HasConversion<int>()
        .HasColumnType("integer");

      e.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("ix_cj_hist_created_at");
      e.HasIndex(x => new { x.Status, x.CreatedAtUtc }).HasDatabaseName("ix_cj_hist_status_date");
      e.HasIndex(x => x.TxID).HasDatabaseName("ix_cj_hist_txid");
    });


    modelBuilder.Entity<RoomEntity>(entity =>
    {
      entity.Property(e => e.CreatedAt)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      entity.Property(e => e.LastMessageTime)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
    });

    modelBuilder.Entity<MessageEntity>(entity =>
    {
      entity.Property(e => e.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      entity.HasOne(m => m.Room)
        .WithMany(r => r.Messages)
        .HasForeignKey(m => m.RoomDealId)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(m => m.Account)
        .WithMany(a => a.Messages)
        .HasForeignKey(m => m.AccountId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<AccountEntity>(entity =>
    {
      entity.Property(e => e.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
    });

    modelBuilder.Entity<TelegramLinkEntity>(entity =>
    {
      entity.Property(e => e.ExpiredAt)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC' + INTERVAL '1 day'");
    });

    modelBuilder.Entity<TvlSnapshotEntity>()
      .HasKey(x => new { x.TakenAt, x.TokenMint });

    modelBuilder.Entity<AssetVolumeDailyEntity>()
      .HasKey(x => new { x.Day, x.TokenMint });

    modelBuilder.Entity<OrderStatusDailyEntity>()
      .HasKey(e => new { e.Day, e.Status });

    modelBuilder.Entity<DealTimeDailyEntity>()
      .HasKey(e => new { e.Day, e.TokenMint });

    modelBuilder.Entity<UserMetricsDailyEntity>()
      .HasKey(x => x.Day);

    modelBuilder.Entity<FunnelMetricsDailyEntity>()
      .HasKey(x => x.Day);

    modelBuilder.Entity<EventEntity>(b =>
    {
      b.Property(x => x.Payload).HasColumnType("jsonb");

      b.Property(x => x.EventType)
        .HasConversion<string>()
        .HasColumnName("event_type")
        .HasMaxLength(40);
    });

    modelBuilder.Entity<OutboxMessage>(e =>
    {
      e.Property(x => x.Payload).HasColumnType("jsonb");
      e.HasIndex(x => x.ProcessedAt);

      e.Property(x => x.Type)
        .HasConversion<string>()
        .HasColumnName("type");
    });


    modelBuilder.Entity<SessionEntity>()
      .HasIndex(x => x.LastSeenAt);

    modelBuilder.Entity<OrderCreatedDailyEntity>()
      .HasKey(x => new { x.Day, x.Side });

    modelBuilder.Entity<AggregatorState>()
      .HasKey(x => new { x.Key });

    modelBuilder.Entity<RefreshTokenEntity>()
      .HasKey(x => new { x.Id });

    modelBuilder.Entity<RatingReviewEntity>()
      .HasKey(x => new { x.Id });


    modelBuilder.Entity<PaymentCategory>(e =>
    {
      e.HasKey(x => x.Id);

      e.Property(x => x.Name)
        .HasMaxLength(64)
        .IsRequired();

      e.HasIndex(x => x.Name).IsUnique();
    });


    modelBuilder.Entity<PaymentMethod>(e =>
    {
      e.HasKey(x => x.Id);

      e.Property(x => x.Code)
        .HasMaxLength(32)
        .IsRequired();

      e.HasIndex(x => x.Code).IsUnique();

      e.Property(x => x.Name)
        .HasMaxLength(64)
        .IsRequired();

      e.HasOne(m => m.Category)
        .WithMany(c => c.Methods)
        .HasForeignKey(m => m.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<Currency>(e =>
    {
      e.HasKey(x => x.Id);

      e.Property(x => x.Code)
        .HasMaxLength(32)
        .IsRequired();

      e.HasIndex(x => x.Code).IsUnique();

      e.Property(x => x.Name)
        .HasMaxLength(64)
        .IsRequired();
    });

    modelBuilder.Entity<PaymentPopularityDaily>(e =>
    {
      e.HasKey(x => new { x.Day, x.MethodId, x.Region });

      e.Property(x => x.Region)
        .HasMaxLength(2)
        .IsRequired();

      e.HasIndex(x => new { x.Region, x.Day, x.Count });
    });
  }
}