using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("order_status_daily")]
public class OrderStatusDailyEntity
{
  [Key, Column("day")] public DateTime Day { get; set; } // date
  [Key, Column("status")] public UniversalOrderStatus Status { get; set; }
  [Column("cnt")] public int Cnt { get; set; }
}