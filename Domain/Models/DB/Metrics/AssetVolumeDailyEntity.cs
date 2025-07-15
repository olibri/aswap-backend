using System.ComponentModel.DataAnnotations.Schema;

[Table("asset_volume_daily")]
public class AssetVolumeDailyEntity
{
  // composite PK: day + token_mint
  [Column("day")] public DateTime Day { get; set; } // date
  [Column("token_mint")] public string TokenMint { get; set; }

  [Column("volume", TypeName = "numeric(38,0)")]
  public decimal Volume { get; set; }
}