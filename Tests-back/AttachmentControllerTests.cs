using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Interfaces.Services.Storage.Attachments;
using Domain.Models.Api.Attachments;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.Attachments;

namespace Tests_back;

public class AttachmentControllerTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task CreateAttachment_ReturnsUploadAndDownloadUrls()
  {
    // Arrange
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    var dto = AttachmentTestExtensions.CreateTestDto(messageId);

    // Act
    var result = await controller.CreateAttachment(dto, CancellationToken.None)
        .OkValueAsync<AttachmentUrlDto>();

    // Assert
    result.ShouldNotBeNull();
    result.AttachmentId.ShouldNotBe(Guid.Empty);
    result.UploadUrl.ShouldNotBeNullOrWhiteSpace();
    result.DownloadUrl.ShouldNotBeNullOrWhiteSpace();
    result.UploadUrl.ShouldContain("X-Amz-Algorithm");
    result.DownloadUrl.ShouldContain("X-Amz-Algorithm");

    result.UploadUrl.ShouldContain("65.109.86.205:9000");
    result.DownloadUrl.ShouldContain("65.109.86.205:9000");
    result.UploadUrl.ShouldContain("/attachments/");
    result.DownloadUrl.ShouldContain("/attachments/");
  }

  [Fact]
  public async Task GetAttachment_ReturnsAttachmentInfo()
  {
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var service = fixture.GetService<IAttachmentService>();

    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    var created = await service.CreateTestAttachmentAsync(messageId);

    // Act
    var result = await controller.GetAttachment(created.AttachmentId, CancellationToken.None)
        .OkValueAsync<AttachmentResponseDto>();

    // Assert
    result.ShouldNotBeNull();
    result.Id.ShouldBe(created.AttachmentId);
    result.MessageId.ShouldBe(messageId);
    result.Bucket.ShouldBe(BucketEnum.Attachments);
    result.Status.ShouldBe(PhotoStatus.Uploading);
    result.MimeType.ShouldBe("image/jpeg");
    result.SizeBytes.ShouldBe(12345);
  }

  [Fact]
  public async Task GetAttachment_NotFound_Returns404()
  {
    // Arrange
    var controller = fixture.GetService<AttachmentController>();
    var nonExistentId = Guid.NewGuid();

    // Act
    var result = await controller.GetAttachment(nonExistentId, CancellationToken.None);

    // Assert
    result.ShouldBeNotFound();
  }

  [Fact]
  public async Task GetMessageAttachments_ReturnsAllAttachmentsForMessage()
  {
    // Arrange
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var service = fixture.GetService<IAttachmentService>();

    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    var otherMessageId = await fixture.CreateMessageForAttachmentTestAsync();

    // Create attachments for our message
    await service.CreateTestAttachmentAsync(messageId, fileName: "image1.jpg");
    await service.CreateTestAttachmentAsync(messageId, fileName: "image2.png");

    // Create attachment for different message
    await service.CreateTestAttachmentAsync(otherMessageId, fileName: "other.gif");

    // Act
    var result = await controller.GetMessageAttachments(messageId, CancellationToken.None)
        .OkValueAsync<IReadOnlyList<AttachmentResponseDto>>();

    // Assert
    result.ShouldNotBeNull();
    result.Count.ShouldBe(2);
    result.All(a => a.MessageId == messageId).ShouldBeTrue();
    result.Select(a => a.StorageKey).ShouldAllBe(key => key.Contains(".jpg") || key.Contains(".png"));
  }

  [Fact]
  public async Task MarkAsUploaded_UpdatesStatusToReady()
  {
    // Arrange
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var service = fixture.GetService<IAttachmentService>();

    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    var created = await service.CreateTestAttachmentAsync(messageId);

    // Act
    var result = await controller.MarkAsUploaded(created.AttachmentId, CancellationToken.None);

    // Assert
    result.ShouldBeNoContent();

    // Verify status changed
    var updated = await service.GetAttachmentAsync(created.AttachmentId);
    updated.ShouldNotBeNull();
    updated!.Status.ShouldBe(PhotoStatus.Ready);
    updated.UploadedAt.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetDownloadUrl_ReadyAttachment_ReturnsUrl()
  {
    // Arrange
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var service = fixture.GetService<IAttachmentService>();


    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    var created = await service.CreateTestAttachmentAsync(messageId);


    // Mark as uploaded first
    await service.MarkAsUploadedAsync(created.AttachmentId);

    // Act
    var result = await controller.GetDownloadUrl(created.AttachmentId, CancellationToken.None)
        .OkValueAsync<object>();

    // Assert
    result.ShouldNotBeNull();
    var downloadUrl = result.GetType().GetProperty("downloadUrl")?.GetValue(result) as string;
    downloadUrl.ShouldNotBeNullOrWhiteSpace();
    downloadUrl.ShouldContain("X-Amz-Algorithm");
    downloadUrl.ShouldContain("65.109.86.205:9000");
  }

  [Fact]
  public async Task GetDownloadUrl_UploadingAttachment_ReturnsBadRequest()
  {
    // Arrange
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var service = fixture.GetService<IAttachmentService>();
    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    var created = await service.CreateTestAttachmentAsync(messageId);


    // Act (don't mark as uploaded)
    var result = await controller.GetDownloadUrl(created.AttachmentId, CancellationToken.None);

    // Assert
    result.ShouldBeOfType<BadRequestObjectResult>();
  }

  //[Fact]
  //public async Task DeleteAttachment_RemovesAttachment()
  //{
  //  // Arrange
  //  fixture.ResetDb("attachments");
  //  var controller = fixture.GetService<AttachmentController>();
  //  var service = fixture.GetService<IAttachmentService>();

  //  var messageId = await fixture.CreateMessageForAttachmentTestAsync();
  //  var created = await service.CreateTestAttachmentAsync(messageId);


  //  // Act
  //  var result = await controller.DeleteAttachment(created.AttachmentId, CancellationToken.None);

  //  // Assert
  //  result.ShouldBeNoContent();

  //  // Verify attachment is deleted
  //  var deleted = await service.GetAttachmentAsync(created.AttachmentId);
  //  deleted.ShouldBeNull();
  //}

  [Fact]
  public async Task CreateAttachment_DifferentBuckets_UseCorrectBucket()
  {
    // Arrange
    fixture.ResetDb("attachments", "messages", "rooms");
    var controller = fixture.GetService<AttachmentController>();
    var messageId = await fixture.CreateMessageForAttachmentTestAsync();
    
    var evidenceDto = AttachmentTestExtensions.CreateTestDto(messageId, BucketEnum.Evidence, "evidence.pdf");
    var thumbnailDto = AttachmentTestExtensions.CreateTestDto(messageId, BucketEnum.Thumbnails, "thumb.jpg");

    // Act
    var evidenceResult = await controller.CreateAttachment(evidenceDto, CancellationToken.None)
        .OkValueAsync<AttachmentUrlDto>();
    var thumbnailResult = await controller.CreateAttachment(thumbnailDto, CancellationToken.None)
        .OkValueAsync<AttachmentUrlDto>();

    // Assert
    evidenceResult.UploadUrl.ShouldContain("evidence");
    thumbnailResult.UploadUrl.ShouldContain("thumbnails");
  }
}