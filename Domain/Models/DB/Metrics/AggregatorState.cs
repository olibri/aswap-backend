using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("aggregator_state")]
public class AggregatorState
{
  [Key] [Column("key")] public string Key { get; set; }
  [Column("value")] public DateTime Value { get; set; }
}