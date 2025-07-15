using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("bans")]
public class BanEntity
{
  [Key] [Column("id")] public long Id { get; set; }
  [Column("wallet")] public string? Wallet { get; set; }
  [Column("telegram_id")] public string? TelegramId { get; set; }
  [Column("reason")] public string Reason { get; set; }
  [Column("banned_at")] public DateTime BannedAt { get; set; }
  [Column("until")] public DateTime? Until { get; set; }
}

