using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("sessions")]
public class SessionEntity
{
  [Key] [Column("session_id")] public Guid SessionId { get; set; } = Guid.NewGuid();
  [Column("wallet")] public string? Wallet { get; set; }
  [Column("ip")] public string? Ip { get; set; } // inet
  [Column("started_at")] public DateTime StartedAt { get; set; }
  [Column("last_seen_at")] public DateTime LastSeenAt { get; set; }
}