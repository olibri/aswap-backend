using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("account")]
public class AccountEntity
{
    [Key][Column("wallet_address")] public string WalletAddress { get; set; }
    [Column("telegram")] public string? Telegram { get; set; }
    [Column("orders_count")] public int? OrdersCount{ get; set; }
    [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }
    [Column("last_active_time")] public DateTime? LastActiveTime { get; set; }

    public ICollection<MessageEntity> Messages { get; set; }
}