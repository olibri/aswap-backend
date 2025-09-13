using Domain.Models.Api.Attachments;

namespace Domain.Interfaces.Services.Storage.Attachments;

public interface IAttachmentService
{
  Task<AttachmentUrlDto> CreateAttachmentAsync(AttachmentUploadDto dto, CancellationToken ct = default);
  Task<AttachmentResponseDto?> GetAttachmentAsync(Guid id, CancellationToken ct = default);
  Task<IReadOnlyList<AttachmentResponseDto>> GetMessageAttachmentsAsync(Guid messageId, CancellationToken ct = default);
  Task MarkAsUploadedAsync(Guid id, CancellationToken ct = default);
  Task DeleteAttachmentAsync(Guid id, CancellationToken ct = default);
  Task<string> GetDownloadUrlAsync(Guid id, CancellationToken ct = default);
}