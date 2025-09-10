using Domain.Enums;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Services.Notification;

public interface INotificationService
{
  Task<PagedResult<UserNotificationDto>> GetUserNotificationsAsync(
    string userWallet,
    NotificationQuery query,
    CancellationToken ct = default);

  Task<UserNotificationDto> CreateNotificationAsync(
    string userWallet,
    string title,
    string message,
    NotificationType type,
    string? relatedEntityId = null,
    string? metadata = null,
    CancellationToken ct = default);

  Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);
  Task MarkAllAsReadAsync(string userWallet, CancellationToken ct = default);

  Task<int> GetUnreadCountAsync(string userWallet, CancellationToken ct = default);

  Task<IEnumerable<UserNotificationDto>> GetRealtimeNotificationsAsync(
    string userWallet,
    DateTime since,
    CancellationToken ct = default);
}