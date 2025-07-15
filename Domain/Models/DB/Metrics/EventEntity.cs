using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("events")]
public class EventEntity
{
  [Key] [Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
  [Column("ts")] public DateTime Ts { get; set; }
  [Column("event_type")] public string EventType { get; set; }
  [Column("wallet")] public string? Wallet { get; set; }
  [Column("ip")] public string? Ip { get; set; } // inet в PG

  [Column("payload", TypeName = "jsonb")]
  public string Payload { get; set; }
}