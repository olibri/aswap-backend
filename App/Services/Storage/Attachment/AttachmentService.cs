using Domain.Enums;
using Domain.Interfaces.Services.Storage;
using Domain.Interfaces.Services.Storage.Attachments;
using Domain.Models.Api.Attachments;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Storage.Attachment;

public sealed class AttachmentService(
  IDbContextFactory<P2PDbContext> dbFactory,
  IMinioService minioService) : IAttachmentService
{
  public async Task<AttachmentUrlDto> CreateAttachmentAsync(AttachmentUploadDto dto, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var fileExtension = GetFileExtension(dto.FileName, dto.MimeType);
    var storageKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}{fileExtension}";

    var attachment = CreateAttachmentEntity(dto, storageKey);

    db.Attachments.Add(attachment);
    await db.SaveChangesAsync(ct);

    var uploadUrl = await minioService.GeneratePresignedUploadUrlAsync(
      dto.Bucket, storageKey, TimeSpan.FromMinutes(15));

    var downloadUrl = await minioService.GeneratePresignedDownloadUrlAsync(
      dto.Bucket, storageKey, TimeSpan.FromHours(1));

    return new AttachmentUrlDto(uploadUrl, downloadUrl, attachment.Id);
  }

  public async Task<AttachmentResponseDto?> GetAttachmentAsync(Guid id, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var attachment = await db.Attachments
      .AsNoTracking()
      .FirstOrDefaultAsync(a => a.Id == id, ct);

    if (attachment == null) return null;

    return await CreateAttachmentResponseDto(attachment);
  }

  public async Task<IReadOnlyList<AttachmentResponseDto>> GetMessageAttachmentsAsync(Guid messageId,
    CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var attachments = await db.Attachments
      .AsNoTracking()
      .Where(a => a.MessageId == messageId)
      .OrderBy(a => a.CreatedAtUtc)
      .ToListAsync(ct);

    var tasks = attachments.Select(CreateAttachmentResponseDto);
    return await Task.WhenAll(tasks);
  }

  public async Task MarkAsUploadedAsync(Guid id, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    await db.Attachments
      .Where(a => a.Id == id)
      .ExecuteUpdateAsync(a => a
        .SetProperty(x => x.Status, PhotoStatus.Ready)
        .SetProperty(x => x.UploadedAtUtc, DateTime.UtcNow), ct);
  }

  public async Task DeleteAttachmentAsync(Guid id, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var attachment = await db.Attachments.FirstOrDefaultAsync(a => a.Id == id, ct);
    if (attachment == null) return;

    await minioService.DeleteObjectAsync(attachment.Bucket, attachment.StorageKey);

    db.Attachments.Remove(attachment);
    await db.SaveChangesAsync(ct);
  }

  public async Task<string> GetDownloadUrlAsync(Guid id, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var attachment = await db.Attachments
      .AsNoTracking()
      .FirstOrDefaultAsync(a => a.Id == id, ct);

    if (attachment?.Status != PhotoStatus.Ready)
      throw new InvalidOperationException("Attachment not ready for download");

    return await minioService.GeneratePresignedDownloadUrlAsync(
      attachment.Bucket, attachment.StorageKey, TimeSpan.FromHours(1));
  }

  private static AttachmentEntity CreateAttachmentEntity(AttachmentUploadDto dto, string storageKey,
    PhotoStatus status = PhotoStatus.Uploading)
  {
    return new AttachmentEntity
    {
      Id = Guid.NewGuid(),
      MessageId = dto.MessageId,
      Bucket = dto.Bucket,
      StorageKey = storageKey,
      MimeType = dto.MimeType,
      SizeBytes = dto.SizeBytes,
      Sha256 = dto.Sha256,
      Width = dto.Width,
      Height = dto.Height,
      DurationMs = dto.DurationMs,
      Status = status,
      CreatedAtUtc = DateTime.UtcNow
    };
  }

  private async Task<AttachmentResponseDto> CreateAttachmentResponseDto(AttachmentEntity attachment)
  {
    var downloadUrl = attachment.Status == PhotoStatus.Ready
      ? await minioService.GeneratePresignedDownloadUrlAsync(
        attachment.Bucket, attachment.StorageKey, TimeSpan.FromHours(1))
      : null;

    return new AttachmentResponseDto(
      attachment.Id,
      attachment.MessageId,
      attachment.Bucket,
      attachment.StorageKey,
      attachment.MimeType,
      attachment.SizeBytes,
      attachment.Status,
      downloadUrl,
      attachment.Width,
      attachment.Height,
      attachment.DurationMs,
      attachment.CreatedAtUtc,
      attachment.UploadedAtUtc
    );
  }

  private static string GetFileExtension(string fileName, string mimeType)
  {
    var ext = Path.GetExtension(fileName);
    if (!string.IsNullOrEmpty(ext)) return ext;

    return mimeType.ToLowerInvariant() switch
    {
      "image/jpeg" or "image/jpg" => ".jpg",
      "image/png" => ".png",
      "image/gif" => ".gif",
      "image/webp" => ".webp",
      "video/mp4" => ".mp4",
      "video/webm" => ".webm",
      "application/pdf" => ".pdf",
      _ => ""
    };
  }
}