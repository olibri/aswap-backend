using System.ComponentModel.DataAnnotations.Schema;

[Table("tvl_snapshots")]
public class TvlSnapshotEntity
{
  // composite PK: taken_at + token_mint
  [Column("taken_at")] public DateTime TakenAt { get; set; }
  [Column("token_mint")] public string TokenMint { get; set; }

  [Column("balance", TypeName = "numeric(38,0)")]
  public decimal Balance { get; set; }
}