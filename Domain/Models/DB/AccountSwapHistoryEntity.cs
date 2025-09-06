using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("account_swap_history")]
public class AccountSwapHistoryEntity
{
  [Key]
  [Column("tx")]
  public string Tx { get; set; } = null!;

  [Column("user_wallet")]
  public string UserWallet { get; set; } = null!;

  [Column("crypto_from")]
  public string CryptoFrom { get; set; } = null!;

  [Column("crypto_to")]
  public string CryptoTo { get; set; } = null!;

  [Column("amount_in", TypeName = "numeric(38,18)")]
  public decimal AmountIn { get; set; }

  [Column("amount_out", TypeName = "numeric(38,18)")]
  public decimal AmountOut { get; set; }

  [Column("price_usd_in", TypeName = "numeric(38,18)")]
  public decimal PriceUsdIn { get; set; }

  [Column("price_usd_out", TypeName = "numeric(38,18)")]
  public decimal PriceUsdOut { get; set; }

  [Column("created_at_utc")]
  public DateTime CreatedAtUtc { get; set; }
}