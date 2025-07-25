using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("rating_reviews")]
public class RatingReviewEntity
{
  [Key][Column("id")] public long Id { get; set; }
  [Column("to_wallet")] public string ToWallet { get; set; } = null!;
  [Column("from_wallet")] public string FromWallet { get; set; } = null!;
  [Column("score", TypeName = "numeric(3,2)")]
  public decimal Score { get; set; } // 0..5
  [Column("comment")] public string? Comment { get; set; }
  [Column("deal_id")] public ulong? DealId { get; set; }
  [Column("created_at_utc")] public DateTime CreatedAt { get; set; }
}