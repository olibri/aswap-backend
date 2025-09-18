using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.DB.CoinPrice;
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
  public DbSet<AttachmentEntity> Attachments { get; set; }
  public DbSet<AccountEntity> Account { get; set; }
  public DbSet<TelegramLinkEntity> TelegramLinks { get; set; }

  /* ─────────── Referral system tables ─────────── */
  public DbSet<ReferralRewardEntity> ReferralRewards { get; set; }
  public DbSet<ReferralStatsDailyEntity> ReferralStatsDaily { get; set; }

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

  /* ─────────── CoinPrice ─────────── */
  public DbSet<PriceSnapshotEntity> PriceSnapshot { get; set; }
  public DbSet<TokenEntity> Tokens { get; set; }
  public DbSet<AppLockEntity> AppLocks { get; set; }

  /* ─────────── SwapHistory ─────────── */
  public DbSet<AccountSwapHistoryEntity> AccountSwapHistory { get; set; }

  public DbSet<ChildOrderEntity> ChildOrders { get; set; }

  public DbSet<UserNotificationEntity> UserNotifications { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<ChildOrderEntity>(entity =>
    {
      entity.HasKey(x => x.Id);

      entity.HasOne(x => x.ParentOrder)
        .WithMany(p => p.ChildOrders)
        .HasForeignKey(x => x.ParentOrderId)
        .HasPrincipalKey(p => p.Id)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasIndex(x => x.ParentOrderId)
        .HasDatabaseName("ix_child_order_parent");

      entity.HasIndex(x => x.DealId)
        .HasDatabaseName("ix_child_order_deal");

      entity.HasIndex(x => new { x.EscrowStatus, x.CreatedAtUtc })
        .HasDatabaseName("ix_child_order_status_created");
    });

    modelBuilder.Entity<UserNotificationEntity>(entity =>
    {
      entity.HasKey(x => x.Id);

      entity.Property(x => x.CreatedAt)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      entity.Property(x => x.NotificationType)
        .HasConversion<string>()
        .HasColumnName("notification_type")
        .HasMaxLength(40);

      entity.Property(x => x.Metadata)
        .HasColumnType("jsonb");

      entity.HasIndex(x => new { x.UserWallet, x.CreatedAt })
        .HasDatabaseName("ix_user_notifications_wallet_created");

      entity.HasIndex(x => new { x.UserWallet, x.IsRead, x.CreatedAt })
        .HasDatabaseName("ix_user_notifications_wallet_read_created");

      entity.HasIndex(x => x.RelatedEntityId)
        .HasDatabaseName("ix_user_notifications_related_entity");

      entity.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserWallet)
        .HasPrincipalKey(a => a.WalletAddress)
        .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<AccountSwapHistoryEntity>(entity =>
    {
      entity.HasKey(x => x.Tx);

      entity.HasIndex(x => new { x.CryptoFrom, x.CreatedAtUtc })
        .HasDatabaseName("ix_swap_from_date")
        .IncludeProperties(x => new { x.PriceUsdIn, x.AmountIn, x.Tx });

      entity.HasIndex(x => new { x.CryptoTo, x.CreatedAtUtc })
        .HasDatabaseName("ix_swap_to_date")
        .IncludeProperties(x => new { x.PriceUsdOut, x.AmountOut, x.Tx });

      entity.HasIndex(x => new { x.CryptoFrom, x.PriceUsdIn, x.CreatedAtUtc })
        .HasDatabaseName("ix_swap_from_price_date");

      entity.HasIndex(x => new { x.CryptoTo, x.PriceUsdOut, x.CreatedAtUtc })
        .HasDatabaseName("ix_swap_to_price_date");

      entity.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("ix_swap_created_at");
    });

    modelBuilder.Entity<EscrowOrderEntity>(entity =>
    {
      entity.Property(e => e.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
      entity.Property(x => x.EscrowStatus)
        .HasConversion<int>()
        .HasColumnType("integer")
        .HasColumnName("escrow_status");
      entity.HasIndex(x => new { x.TokenMint, x.FiatCode, x.OfferSide, Status = x.EscrowStatus, x.Price })
        .HasDatabaseName("ix_escrow_best_price");

      entity.Navigation(x => x.ChildOrders).AutoInclude(false);
    });

    modelBuilder.Entity<AppLockEntity>(e =>
    {
      e.HasKey(x => x.Name);
      e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
      e.HasIndex(x => x.LockedUntilUtc);
    });

    modelBuilder.Entity<AttachmentEntity>(e =>
    {
      e.HasKey(x => x.Id);

      e.Property(x => x.Bucket)
        .HasConversion<int>()
        .HasColumnType("integer");

      e.Property(x => x.Status)
        .HasConversion<int>()
        .HasColumnType("integer")
        .HasDefaultValue(PhotoStatus.Uploading);

      e.Property(x => x.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      e.Property(x => x.Sha256)
        .HasColumnType("bytea");

      e.HasOne(x => x.Message)
        .WithMany(m => m.Attachments)
        .HasForeignKey(x => x.MessageId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(x => new { x.MessageId, x.CreatedAtUtc })
        .HasDatabaseName("ix_attachment_msg_created");

      e.HasIndex(x => new { x.Status, x.CreatedAtUtc })
        .HasDatabaseName("ix_attachment_status_created");

      e.HasIndex(x => x.StorageKey)
        .IsUnique()
        .HasDatabaseName("ux_attachment_storage_key");

      e.HasIndex(x => new { x.Sha256, x.SizeBytes })
        .IsUnique()
        .HasDatabaseName("ux_attachment_sha_size");
    });

    modelBuilder.Entity<PriceSnapshotEntity>(e =>
    {
      e.ToTable("price_snapshot_minute");
      e.HasKey(x => x.Id);
      e.Property(x => x.Id)
        .HasColumnName("id")
        .HasColumnType("uuid")
        .ValueGeneratedOnAdd()
        .HasDefaultValueSql("gen_random_uuid()");
      e.Property(x => x.TokenMint).IsRequired();
      e.Property(x => x.Price).HasColumnType("numeric(38,18)").IsRequired();

      e.Property(x => x.CollectedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      e.HasIndex(x => new { x.TokenMint, x.MinuteBucketUtc })
        .IsUnique()
        .HasDatabaseName("ux_price_minute");

      e.HasIndex(x => x.MinuteBucketUtc)
        .HasDatabaseName("ix_price_minute_bucket");
    });

    modelBuilder.Entity<TokenEntity>(e =>
    {
      e.ToTable("token");
      e.HasKey(x => x.Mint);

      e.Property(x => x.Symbol).HasMaxLength(32);
      e.Property(x => x.Name).HasMaxLength(128);
      e.Property(x => x.Decimals);
      e.Property(x => x.IsVerified);

      e.Property(x => x.CreatedAtUtc)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      // не унікальні, просто допоміжні індекси (опційно)
      e.HasIndex(x => x.Symbol).HasDatabaseName("ix_token_symbol");
      e.HasIndex(x => x.IsVerified).HasDatabaseName("ix_token_verified");
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

      e.HasIndex(x => new { x.CryptoCurrencyName, x.CryptoCurrencyChain })
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

      // Referral code должен быть уникальным
      entity.HasIndex(x => x.ReferralCode)
        .IsUnique()
        .HasDatabaseName("ux_account_referral_code");

      // Индекс для поиска по referrer
      entity.HasIndex(x => x.ReferredBy)
        .HasDatabaseName("ix_account_referred_by");

      // Self-referencing relationship
      entity.HasOne(x => x.MyInviter)
        .WithMany(x => x.MyInvited)
        .HasForeignKey(x => x.ReferredBy)
        .HasPrincipalKey(x => x.WalletAddress)
        .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<ReferralRewardEntity>(entity =>
    {
      entity.HasKey(x => x.Id);

      entity.Property(x => x.CreatedAt)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      // Индексы для оптимизации запросов
      entity.HasIndex(x => new { x.ReferrerWallet, x.CreatedAt })
        .HasDatabaseName("ix_referral_rewards_referrer_created");

      entity.HasIndex(x => new { x.RefereeWallet, x.CreatedAt })
        .HasDatabaseName("ix_referral_rewards_referee_created");

      entity.HasIndex(x => x.OrderId)
        .IsUnique()
        .HasDatabaseName("ux_referral_rewards_order");

      entity.HasIndex(x => new { x.ProcessedAt, x.CreatedAt })
        .HasDatabaseName("ix_referral_rewards_processed_created");

      // Foreign keys
      entity.HasOne(x => x.InviterAccount)
        .WithMany(a => a.EarnedRewards)
        .HasForeignKey(x => x.ReferrerWallet)
        .HasPrincipalKey(a => a.WalletAddress)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(x => x.InvitedAccount)
        .WithMany()
        .HasForeignKey(x => x.RefereeWallet)
        .HasPrincipalKey(a => a.WalletAddress)
        .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(x => x.Order)
        .WithMany()
        .HasForeignKey(x => x.OrderId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<ReferralStatsDailyEntity>(entity =>
    {
      entity.HasKey(x => new { x.Day, x.ReferrerWallet });

      entity.Property(x => x.CreatedAt)
        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

      entity.HasIndex(x => new { x.ReferrerWallet, x.Day })
        .HasDatabaseName("ix_referral_stats_referrer_day");

      entity.HasIndex(x => new { x.Day, x.RewardsEarnedUsd })
        .HasDatabaseName("ix_referral_stats_day_rewards");

      entity.HasIndex(x => new { x.TotalReferrals, x.Day })
        .HasDatabaseName("ix_referral_stats_total_day");

      entity.HasOne(x => x.InviterAccount)
        .WithMany()
        .HasForeignKey(x => x.ReferrerWallet)
        .HasPrincipalKey(a => a.WalletAddress)
        .OnDelete(DeleteBehavior.Cascade);
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