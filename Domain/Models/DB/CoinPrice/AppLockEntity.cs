using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.CoinPrice;

[Table("app_lock")]
public sealed class AppLockEntity
{
  [Key]
  [Column("name")]
  public string Name { get; set; } = null!;

  [Column("locked_until_utc")]
  public DateTime? LockedUntilUtc { get; set; }

  [Column("lock_owner")]
  public Guid? LockOwner { get; set; }

  [Column("created_at_utc")]
  public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
