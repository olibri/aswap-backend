using Domain.Enums;

namespace Domain.Models.Api.Attachments;

public sealed record AttachmentUploadDto(
  Guid MessageId,
  BucketEnum Bucket,
  string FileName,
  string MimeType,
  long SizeBytes,
  byte[] Sha256,
  int? Width = null,
  int? Height = null,
  int? DurationMs = null
);