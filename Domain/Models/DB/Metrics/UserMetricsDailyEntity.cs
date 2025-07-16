using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("user_metrics_daily")]
public class UserMetricsDailyEntity
{
  [Key] [Column("day")] public DateTime Day { get; set; }
  [Column("dau_users")] public int DauUsers { get; set; }
  [Column("dau_ips")] public int DauIps { get; set; }
  [Column("wau_users")] public int WauUsers { get; set; }
  [Column("mau_users")] public int MauUsers { get; set; }
}