using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.PaymentMethod;

[Table("payment_categories")]
public class PaymentCategory
{
  [Key, Column("category_id")] public short Id { get; set; }
  [Column("name")] public string Name { get; set; } = default!;
  public ICollection<PaymentMethod> Methods { get; init; } = [];
}