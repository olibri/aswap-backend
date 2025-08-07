using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Currency;

[Table("currency")]
public class Currency
{
  [Key] [Column("currency_id")] public short Id { get; set; }
  [Column("code")] public string Code { get; set; } = default!; //slug
  [Column("name")] public string Name { get; set; } = default!;
  [Column("is_active")] public bool IsActive { get; set; } = true;
}