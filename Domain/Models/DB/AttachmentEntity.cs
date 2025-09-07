using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models.DB;


[Table("attachments")]
public class AttachmentEntity
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [Column("message_id")]
  public Guid MessageId { get; set; }

  [Required]
  [Column("bucket")]
  public BucketEnum Bucket { get; set; }

  [Required]
  [Column("storage_key")]
  public string StorageKey { get; set; } = default!;

  [Required]
  [Column("mime_type")]
  public string MimeType { get; set; } = default!;

  [Required]
  [Column("size_bytes")]
  public long SizeBytes { get; set; }

  [Required]
  [Column("sha256")]
  public byte[] Sha256 { get; set; } = default!;

  [Column("width")] public int? Width { get; set; }
  [Column("height")] public int? Height { get; set; }
  [Column("duration_ms")] public int? DurationMs { get; set; }

  [Required]
  [Column("status")]
  public PhotoStatus Status { get; set; } 

  [Required]
  [Column("immutable")]
  public bool Immutable { get; set; } = false;

  [Column("retention_until")]
  public DateTime? RetentionUntil { get; set; }

  [Required]
  [Column("created_at_utc")]
  public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

  [Column("uploaded_at_utc")]
  public DateTime? UploadedAtUtc { get; set; }

  [ForeignKey(nameof(MessageId))]
  public MessageEntity Message { get; set; } = default!;
}