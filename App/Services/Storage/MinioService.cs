using Domain.Enums;
using Domain.Interfaces.Services.Storage;
using Minio;
using Minio.DataModel.Args;

namespace App.Services.Storage;

public sealed class MinioService(IMinioClient minioClient) : IMinioService
{
  private static readonly Dictionary<BucketEnum, string> BucketNames = new()
  {
    { BucketEnum.Evidence, "evidence" },
    { BucketEnum.Attachments, "attachments" },
    { BucketEnum.Thumbnails, "thumbnails" }
  };

  public async Task<string> GeneratePresignedUploadUrlAsync(BucketEnum bucket, string objectKey, TimeSpan expiry)
  {
    var args = new PresignedPutObjectArgs()
      .WithBucket(BucketNames[bucket])
      .WithObject(objectKey)
      .WithExpiry((int)expiry.TotalSeconds);

    return await minioClient.PresignedPutObjectAsync(args);
  }

  public async Task<string> GeneratePresignedDownloadUrlAsync(BucketEnum bucket, string objectKey, TimeSpan expiry)
  {
    var args = new PresignedGetObjectArgs()
      .WithBucket(BucketNames[bucket])
      .WithObject(objectKey)
      .WithExpiry((int)expiry.TotalSeconds);

    return await minioClient.PresignedGetObjectAsync(args);
  }

  public async Task<bool> ObjectExistsAsync(BucketEnum bucket, string objectKey)
  {
    try
    {
      var args = new StatObjectArgs()
        .WithBucket(BucketNames[bucket])
        .WithObject(objectKey);

      await minioClient.StatObjectAsync(args);
      return true;
    }
    catch
    {
      return false;
    }
  }

  public async Task DeleteObjectAsync(BucketEnum bucket, string objectKey)
  {
    var args = new RemoveObjectArgs()
      .WithBucket(BucketNames[bucket])
      .WithObject(objectKey);

    await minioClient.RemoveObjectAsync(args);
  }

  public async Task<Stream> GetObjectAsync(BucketEnum bucket, string objectKey)
  {
    var memoryStream = new MemoryStream();

    var args = new GetObjectArgs()
      .WithBucket(BucketNames[bucket])
      .WithObject(objectKey)
      .WithCallbackStream(stream => stream.CopyTo(memoryStream));

    await minioClient.GetObjectAsync(args);
    memoryStream.Position = 0;
    return memoryStream;
  }
}