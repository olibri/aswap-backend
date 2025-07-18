using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models.DB.Metrics;

[Table("order_created_daily")]
[Index(nameof(Day))]               
[Index(nameof(Side))]               
public class OrderCreatedDailyEntity
{
  [Key] [Column("day")] public DateTime Day { get; set; }
  [Column("side")] public OrderSide Side { get; set; }
  [Column("created_cnt")] public int CreatedCnt { get; set; }
}