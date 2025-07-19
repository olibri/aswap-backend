using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("events")]
public class EventEntity
{
  [Key] [Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
  [Column("ts")] public DateTime Ts { get; set; }
  [Column("event_type")] public EventType EventType { get; set; }
  [Column("wallet")] public string? Wallet { get; set; }
  [Column("ip")] public string? Ip { get; set; }

  [Column("payload", TypeName = "jsonb")]
  public string Payload { get; set; }
}