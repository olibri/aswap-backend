using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("asset_volume_daily")]
public class AssetVolumeDailyEntity
{
  [Column("day")] public DateTime Day { get; set; } 
  [Column("token_mint")] public string TokenMint { get; set; }

  [Column("volume", TypeName = "numeric(38,0)")]
  public decimal Volume { get; set; }
}