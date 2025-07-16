using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("ratings")]
public class RatingEntity
{
  [Key] [Column("wallet")] public string Wallet { get; set; }
  [Column("completed")] public int Completed { get; set; }
  [Column("disputes")] public int Disputes { get; set; }
  [Column("positive")] public int Positive { get; set; }

  [Column("score", TypeName = "numeric(5,2)")]
  public decimal Score { get; set; }
}