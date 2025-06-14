using Domain.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class P2PDbContext(DbContextOptions<P2PDbContext> opt) : DbContext(opt)
{
    public DbSet<EscrowOrderEntity> EscrowOrders { get; set; }
    public DbSet<RoomEntity> Rooms { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<AccountEntity> Account { get; set; }
    public DbSet<TelegramLinkEntity> TelegramLinks { get; set; }


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
    }
}