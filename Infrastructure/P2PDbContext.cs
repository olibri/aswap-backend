using Domain.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class P2PDbContext(DbContextOptions<P2PDbContext> opt) : DbContext(opt)
{
    public DbSet<EscrowOrderEntity> EscrowOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EscrowOrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EscrowPda)
                .IsRequired();
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.TxInitSig)
                .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(e => e.DealId)
                .IsRequired();
            entity.Property(e => e.Seller)
                .IsRequired();
            entity.Property(e => e.TokenMint)
                .IsRequired();
            entity.Property(e => e.FiatCode)
                .IsRequired();
            entity.Property(e => e.Amount)
                .IsRequired();
            entity.Property(e => e.Price)
                .IsRequired();
        });
    }
}