using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("auth_refresh")]
public class RefreshTokenEntity
{
  [Key][Column("id")] public long Id { get; set; }

  [Column("wallet")] public string Wallet { get; set; } = null!;

  [Column("refresh_hash")] public string RefreshHash { get; set; } = null!;

  [Column("expires_at_utc")] public DateTime ExpiresAtUtc { get; set; }

  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }

  [Column("revoked_at_utc")] public DateTime? RevokedAtUtc { get; set; }

  [Column("ua")] public string? UserAgent { get; set; }
  [Column("ip")] public string? Ip { get; set; }
}