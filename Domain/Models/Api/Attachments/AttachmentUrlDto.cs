namespace Domain.Models.Api.Attachments;

public sealed record AttachmentUrlDto(
  string UploadUrl,
  string DownloadUrl,
  Guid AttachmentId
);