using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("deal_time_daily")]
public class DealTimeDailyEntity
{
  [Key] public DateTime Day { get; set; }
  [Key] public string TokenMint { get; set; }

  public double AvgSeconds { get; set; }
  public int TradeCnt { get; set; }     
}