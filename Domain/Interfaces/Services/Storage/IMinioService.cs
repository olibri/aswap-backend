using Domain.Enums;

namespace Domain.Interfaces.Services.Storage;

public interface IMinioService
{
  Task<string> GeneratePresignedUploadUrlAsync(BucketEnum bucket, string objectKey, TimeSpan expiry);
  Task<string> GeneratePresignedDownloadUrlAsync(BucketEnum bucket, string objectKey, TimeSpan expiry);
  Task<bool> ObjectExistsAsync(BucketEnum bucket, string objectKey);
  Task DeleteObjectAsync(BucketEnum bucket, string objectKey);
  Task<Stream> GetObjectAsync(BucketEnum bucket, string objectKey);
}