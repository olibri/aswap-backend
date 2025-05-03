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
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(e => e.Status)
                .HasConversion<string>();
        });
    }
}