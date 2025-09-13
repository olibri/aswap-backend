using Domain.Interfaces.Services.Storage.Attachments;
using Domain.Models.Api.Attachments;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;


[ApiController]
[Route("api/attachments")]
//[Authorize]
public class AttachmentController(IAttachmentService attachmentService) : Controller
{
  /// <summary>
  /// Creates an attachment and returns URLs for upload and download
  /// </summary>
  /// <param name="dto">Attachment upload information</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Upload and download URLs for the attachment</returns>
  /// <response code="200">Returns the upload and download URLs</response>
  /// <response code="400">If the request is invalid</response>
  /// <response code="401">If the user is not authenticated</response>
  [HttpPost]
  [ProducesResponseType(typeof(AttachmentUrlDto), 200)]
  [ProducesResponseType(400)]
  [ProducesResponseType(401)]
  public async Task<IActionResult> CreateAttachment([FromBody] AttachmentUploadDto dto, CancellationToken ct)
  {
    var result = await attachmentService.CreateAttachmentAsync(dto, ct);
    return Ok(result);
  }

  /// <summary>
  /// Gets attachment information by ID
  /// </summary>
  /// <param name="id">Attachment ID</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Attachment information</returns>
  /// <response code="200">Returns the attachment information</response>
  /// <response code="404">If the attachment is not found</response>
  /// <response code="401">If the user is not authenticated</response>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(AttachmentResponseDto), 200)]
  [ProducesResponseType(404)]
  [ProducesResponseType(401)]
  public async Task<IActionResult> GetAttachment(Guid id, CancellationToken ct)
  {
    var attachment = await attachmentService.GetAttachmentAsync(id, ct);
    if (attachment == null) return NotFound();

    return Ok(attachment);
  }

  /// <summary>
  /// Gets all attachments for a specific message
  /// </summary>
  /// <param name="messageId">Message ID</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>List of attachments for the message</returns>
  /// <response code="200">Returns the list of attachments</response>
  /// <response code="401">If the user is not authenticated</response>
  [HttpGet("message/{messageId:guid}")]
  [ProducesResponseType(typeof(IReadOnlyList<AttachmentResponseDto>), 200)]
  [ProducesResponseType(401)]
  public async Task<IActionResult> GetMessageAttachments(Guid messageId, CancellationToken ct)
  {
    var attachments = await attachmentService.GetMessageAttachmentsAsync(messageId, ct);
    return Ok(attachments);
  }

  /// <summary>
  /// Marks an attachment as uploaded (called after successful upload to MinIO)
  /// </summary>
  /// <param name="id">Attachment ID</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>No content</returns>
  /// <response code="204">Attachment successfully marked as uploaded</response>
  /// <response code="404">If the attachment is not found</response>
  /// <response code="401">If the user is not authenticated</response>
  [HttpPut("{id:guid}/uploaded")]
  [ProducesResponseType(204)]
  [ProducesResponseType(404)]
  [ProducesResponseType(401)]
  public async Task<IActionResult> MarkAsUploaded(Guid id, CancellationToken ct)
  {
    await attachmentService.MarkAsUploadedAsync(id, ct);
    return NoContent();
  }

  /// <summary>
  /// Gets a download URL for an attachment
  /// </summary>
  /// <param name="id">Attachment ID</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Download URL for the attachment</returns>
  /// <response code="200">Returns the download URL</response>
  /// <response code="400">If the attachment is not ready for download</response>
  /// <response code="404">If the attachment is not found</response>
  /// <response code="401">If the user is not authenticated</response>
  [HttpGet("{id:guid}/download")]
  [ProducesResponseType(typeof(object), 200)]
  [ProducesResponseType(400)]
  [ProducesResponseType(404)]
  [ProducesResponseType(401)]
  public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken ct)
  {
    try
    {
      var url = await attachmentService.GetDownloadUrlAsync(id, ct);
      return Ok(new { downloadUrl = url });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(ex.Message);
    }
  }

  /// <summary>
  /// Deletes an attachment
  /// </summary>
  /// <param name="id">Attachment ID</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>No content</returns>
  /// <response code="204">Attachment successfully deleted</response>
  /// <response code="404">If the attachment is not found</response>
  /// <response code="401">If the user is not authenticated</response>
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(204)]
  [ProducesResponseType(404)]
  [ProducesResponseType(401)]
  public async Task<IActionResult> DeleteAttachment(Guid id, CancellationToken ct)
  {
    await attachmentService.DeleteAttachmentAsync(id, ct);
    return NoContent();
  }
}