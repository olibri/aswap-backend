using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("outbox_messages")]
public class OutboxMessage
{
  [Key] public Guid Id { get; set; }
  [Column("type")] public EventType Type { get; set; } 
  [Column("occurred_at")] public DateTime OccurredAt { get; set; }
  [Column("payload", TypeName = "jsonb")]
  public string Payload { get; set; }
  [Column("processed_at")] public DateTime? ProcessedAt { get; set; }
}