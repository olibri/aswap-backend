using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models.DB;

[Table("coin_jelly_account_history_entity")]
public class CoinJellyAccountHistoryEntity
{
  [Key][Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
  
  [Column("tx_id")] public string TxID{ get; set; }
  [Column("user_wallet")] public string UserWallet{ get; set; }
  [Column("crypto_send")] public string CryptoSend { get; set; }
  [Column("crypto_get")] public string CryptoGet { get; set; }
  [Column("crypto_get_chain")] public string CryptoGetChain { get; set; }

  [Column("amount_send", TypeName = "numeric(20,0)")] public decimal AmountSend { get; set; }
  [Column("amount_get", TypeName = "numeric(20,0)")] public decimal AmountGet { get; set; }

  [Column("fee_atomic", TypeName = "numeric(78,0)")] public decimal FeeAtomic { get; set; }

  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }
  [Column("status")] public CoinJellyStatus Status { get; set; }
}