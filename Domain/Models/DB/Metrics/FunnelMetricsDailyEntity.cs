using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("funnel_metrics_daily")]
public class FunnelMetricsDailyEntity
{
  [Key] [Column("day")] public DateTime Day { get; set; }
  [Column("connects_cnt")] public int ConnectsCnt { get; set; }
  [Column("orders_cnt")] public int OrdersCnt { get; set; }
  [Column("trades_cnt")] public int TradesCnt { get; set; }
}