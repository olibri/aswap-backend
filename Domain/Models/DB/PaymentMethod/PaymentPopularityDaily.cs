using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.PaymentMethod;

[Table("payment_popularity_daily")]
public class PaymentPopularityDaily
{
  [Key, Column("day")] public DateOnly Day { get; set; }
  [Key, Column("method_id")] public short MethodId { get; set; }
  [Key, Column("region")] public string Region { get; set; } = default!;
  [Column("cnt")] public int Count { get; set; }
}