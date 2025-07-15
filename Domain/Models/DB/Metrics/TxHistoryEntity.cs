using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("tx_history")]
public class TxHistoryEntity
{
  [Key] [Column("tx_hash")] public string TxHash { get; set; }
  [Column("wallet")] public string? Wallet { get; set; }
  [Column("token_mint")] public string? TokenMint { get; set; }

  [Column("amount", TypeName = "numeric(38,0)")]
  public decimal Amount { get; set; }

  [Column("side")] public string Side { get; set; } // BUY / SELL
  [Column("ts")] public DateTime Ts { get; set; }
}