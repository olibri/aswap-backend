using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.CoinPrice;

[Table("token")]
public class TokenEntity
{
  [Key] [Column("mint")] public string Mint { get; set; } = null!;
  [Column("symbol")] public string? Symbol { get; set; }
  [Column("name")] public string? Name { get; set; }
  [Column("decimals")] public int? Decimals { get; set; }
  [Column("is_verified")] public bool? IsVerified { get; set; }
  [Column("icon")] public string? Icon { get; set; }

  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}