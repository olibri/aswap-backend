using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models.DB;

[Table("user_notifications")]
public class UserNotificationEntity
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [Column("user_wallet")]
  [MaxLength(64)]
  public string UserWallet { get; set; } = default!;

  [Required]
  [Column("title")]
  [MaxLength(200)]
  public string Title { get; set; } = default!;

  [Required]
  [Column("message")]
  [MaxLength(1000)]
  public string Message { get; set; } = default!;

  [Column("notification_type")]
  public NotificationType NotificationType { get; set; }

  [Column("related_entity_id")]
  public string? RelatedEntityId { get; set; }

  [Column("is_read")]
  public bool IsRead { get; set; } = false;

  [Column("created_at")]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  [Column("read_at")]
  public DateTime? ReadAt { get; set; }

  [Column("metadata", TypeName = "jsonb")]
  public string? Metadata { get; set; }

  // Навігація до користувача
  [ForeignKey(nameof(UserWallet))]
  public AccountEntity? User { get; set; }
}