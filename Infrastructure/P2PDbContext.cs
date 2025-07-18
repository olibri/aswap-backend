using Domain.Models.DB;
using Domain.Models.DB.Metrics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class P2PDbContext(DbContextOptions<P2PDbContext> opt) : DbContext(opt)
{
    /* ─────────── Core tables ─────────── */
    public DbSet<EscrowOrderEntity> EscrowOrders { get; set; }
    public DbSet<RoomEntity> Rooms { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<AccountEntity> Account { get; set; }
    public DbSet<TelegramLinkEntity> TelegramLinks { get; set; }

    /* ─────────── Metrics tables ─────────── */
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DbSet<TvlSnapshotEntity> TvlSnapshots { get; set; }
    public DbSet<AssetVolumeDailyEntity> AssetVolumeDaily { get; set; }
    public DbSet<OrderStatusDailyEntity> OrderStatusDaily { get; set; }
    public DbSet<UserMetricsDailyEntity> UserMetricsDaily { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<FunnelMetricsDailyEntity> FunnelMetricsDaily { get; set; }
    public DbSet<RatingEntity> Ratings { get; set; }
    public DbSet<TxHistoryEntity> TxHistory { get; set; }
    public DbSet<BanEntity> Bans { get; set; }
    public DbSet<AggregatorState> AggregatorStates { get; set; }
    public DbSet<OrderCreatedDailyEntity> OrderCreatedDaily { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EscrowOrderEntity>(entity =>
        {
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(e => e.Status)
                .HasConversion<string>();
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
            .HasKey(x => x.Day);

        modelBuilder.Entity<UserMetricsDailyEntity>()
            .HasKey(x => x.Day);

        modelBuilder.Entity<FunnelMetricsDailyEntity>()
            .HasKey(x => x.Day);

        modelBuilder.Entity<EventEntity>()
            .Property(x => x.Payload)
            .HasColumnType("jsonb");

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.Property(x => x.Payload).HasColumnType("jsonb");
            e.HasIndex(x => x.ProcessedAt);
        });


        modelBuilder.Entity<SessionEntity>()
            .HasIndex(x => x.LastSeenAt);

        modelBuilder.Entity<OrderCreatedDailyEntity>()
            .HasKey(x => new { x.Day, x.Side });

        modelBuilder.Entity<AggregatorState>()
            .HasKey(x => new { x.Key });

    }
}