using Domain.Enums;

namespace Domain.Models.Api.Attachments;

public sealed record AttachmentResponseDto(
  Guid Id,
  Guid MessageId,
  BucketEnum Bucket,
  string StorageKey,
  string MimeType,
  long SizeBytes,
  PhotoStatus Status,
  string? DownloadUrl = null,
  int? Width = null,
  int? Height = null,
  int? DurationMs = null,
  DateTime CreatedAt = default,
  DateTime? UploadedAt = null
);