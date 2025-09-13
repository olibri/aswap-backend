using System.Security.Cryptography;
using System.Text;
using Domain.Enums;
using Domain.Interfaces.Services.Storage.Attachments;
using Domain.Models.Api.Attachments;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Tests_back.Extensions.Attachments;

public static class AttachmentTestExtensions
{
  public static async Task<AttachmentUrlDto> CreateTestAttachmentAsync(
    this IAttachmentService service,
    Guid messageId,
    BucketEnum bucket = BucketEnum.Attachments,
    string fileName = "test.jpg",
    string mimeType = "image/jpeg",
    long sizeBytes = 12345)
  {
    var sha256 = ComputeSha256Hash(fileName);

    var dto = new AttachmentUploadDto(
      MessageId: messageId,
      Bucket: bucket,
      FileName: fileName,
      MimeType: mimeType,
      SizeBytes: sizeBytes,
      Sha256: sha256,
      Width: 800,
      Height: 600
    );

    return await service.CreateAttachmentAsync(dto);
  }

  public static async Task<T> OkValueAsync<T>(this Task<IActionResult> actionTask)
  {
    var result = await actionTask;
    var okResult = result as OkObjectResult;
    okResult.ShouldNotBeNull();
    return (T)okResult.Value!;
  }

  public static void ShouldBeNoContent(this IActionResult result)
  {
    result.ShouldBeOfType<NoContentResult>();
  }

  public static void ShouldBeNotFound(this IActionResult result)
  {
    result.ShouldBeOfType<NotFoundResult>();
  }

  private static byte[] ComputeSha256Hash(string input)
  {
    return SHA256.HashData(Encoding.UTF8.GetBytes(input));
  }

  public static AttachmentUploadDto CreateTestDto(
    Guid messageId,
    BucketEnum bucket = BucketEnum.Attachments,
    string fileName = "eat.png")
  {
    var sha256 = ComputeSha256Hash(fileName);

    return new AttachmentUploadDto(
      MessageId: messageId,
      Bucket: bucket,
      FileName: fileName,
      MimeType: "image/png",
      SizeBytes: 54321,
      Sha256: sha256,
      Width: 1024,
      Height: 768
    );
  }
}