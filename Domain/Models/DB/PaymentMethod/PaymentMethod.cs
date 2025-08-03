using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.PaymentMethod;

[Table("payment_methods")]
public class PaymentMethod
{
  [Key] [Column("method_id")] public short Id { get; set; }
  [Column("code")] public string Code { get; set; } = default!; //slug
  [Column("name")] public string Name { get; set; } = default!;
  [Column("is_active")] public bool IsActive { get; set; } = true;

  [Column("category_id")] public short CategoryId { get; set; }
  public PaymentCategory Category { get; set; } = default!;
}