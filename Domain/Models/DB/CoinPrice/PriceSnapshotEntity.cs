using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models.DB.CoinPrice;

[Index(nameof(TokenMint), nameof(Quote),
       nameof(MinuteBucketUtc), IsUnique = true, Name = "ux_price_minute")]
[Index(nameof(MinuteBucketUtc), Name = "ix_price_minute_bucket")]
[Index(nameof(TokenMint), nameof(Quote), Name = "ix_price_token_quote")]
[Table("price_snapshot_minute")]
public class PriceSnapshotEntity
{
  [Key] [Column("id")] public long Id { get; set; }

  [Column("token_mint")] public string TokenMint { get; set; } = null!;
  [Column("quote")] public string Quote { get; set; } = " ";

  [Column("price", TypeName = "numeric(38,18)")]
  public decimal Price { get; set; }

  [Column("minute_bucket_utc")] public DateTime MinuteBucketUtc { get; set; }

  [Column("collected_at_utc")] public DateTime CollectedAtUtc { get; set; }
}