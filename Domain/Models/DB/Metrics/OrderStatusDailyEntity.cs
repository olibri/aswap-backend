using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("order_status_daily")]
public class OrderStatusDailyEntity
{
  [Key] [Column("day")] public DateTime Day { get; set; } // date
  [Column("open_cnt")] public int OpenCnt { get; set; }
  [Column("filled_cnt")] public int FilledCnt { get; set; }
  [Column("cancelled_cnt")] public int CancelledCnt { get; set; }
}