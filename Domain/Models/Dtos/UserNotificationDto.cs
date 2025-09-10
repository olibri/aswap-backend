using Domain.Enums;

namespace Domain.Models.Dtos;

public record UserNotificationDto(
  Guid Id,
  string UserWallet,
  string Title,
  string Message,
  NotificationType NotificationType,
  string? RelatedEntityId,
  bool IsRead,
  DateTime CreatedAt,
  DateTime? ReadAt,
  string? Metadata
);