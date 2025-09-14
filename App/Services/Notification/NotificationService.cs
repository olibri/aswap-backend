using Domain.Enums;
using Domain.Interfaces.Services.Notification;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Notification;

public class NotificationService(IDbContextFactory<P2PDbContext> dbFactory) : INotificationService
{
  public async Task<PagedResult<UserNotificationDto>> GetUserNotificationsAsync(
    string userWallet,
    NotificationQuery query,
    CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var baseQuery = db.UserNotifications
      .AsNoTracking()
      .Where(n => n.UserWallet == userWallet);

    // Фільтри
    if (query.IsRead.HasValue)
      baseQuery = baseQuery.Where(n => n.IsRead == query.IsRead.Value);

    if (query.FromDate.HasValue)
      baseQuery = baseQuery.Where(n => n.CreatedAt >= query.FromDate.Value);

    if (query.ToDate.HasValue)
      baseQuery = baseQuery.Where(n => n.CreatedAt <= query.ToDate.Value);

    // Загальна кількість
    var total = await baseQuery.CountAsync(ct);

    // ✅ Використовуємо query.Skip і query.Take (що автоматично рахуються)
    var notifications = await baseQuery
      .OrderByDescending(n => n.CreatedAt)
      .Skip(query.Skip)
      .Take(query.Take)
      .Select(n => new UserNotificationDto(
        n.Id,
        n.UserWallet,
        n.Title,
        n.Message,
        n.NotificationType,
        n.RelatedEntityId,
        n.IsRead,
        n.CreatedAt,
        n.ReadAt,
        n.Metadata))
      .ToListAsync(ct);

    return new PagedResult<UserNotificationDto>(
      notifications,
      query.Page,
      query.Size,
      total
    );
  }
  public async Task<UserNotificationDto> CreateNotificationAsync(
    string userWallet,
    string title,
    string message,
    NotificationType type,
    string? relatedEntityId = null,
    string? metadata = null,
    CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var notification = new UserNotificationEntity
    {
      UserWallet = userWallet,
      Title = title,
      Message = message,
      NotificationType = type,
      RelatedEntityId = relatedEntityId,
      Metadata = metadata
    };

    db.Set<UserNotificationEntity>().Add(notification);
    await db.SaveChangesAsync(ct);

    // TODO: Тут буде місце для відправки real-time нотифікації через сокети
    // await _hubContext.Clients.User(userWallet).SendAsync("NewNotification", ToDto(notification), ct);

    return ToDto(notification);
  }

  public async Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var notification = await db.Set<UserNotificationEntity>()
      .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

    if (notification is null)
      throw new InvalidOperationException($"Notification with id {notificationId} not found");

    notification.IsRead = true;
    notification.ReadAt = DateTime.UtcNow;

    await db.SaveChangesAsync(ct);
  }

  public async Task MarkAllAsReadAsync(string userWallet, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    await db.Set<UserNotificationEntity>()
      .Where(n => n.UserWallet == userWallet && !n.IsRead)
      .ExecuteUpdateAsync(n => n
        .SetProperty(x => x.IsRead, true)
        .SetProperty(x => x.ReadAt, DateTime.UtcNow), ct);
  }

  public async Task<int> GetUnreadCountAsync(string userWallet, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    return await db.Set<UserNotificationEntity>()
      .CountAsync(n => n.UserWallet == userWallet && !n.IsRead, ct);
  }

  public async Task<IEnumerable<UserNotificationDto>> GetRealtimeNotificationsAsync(
    string userWallet,
    DateTime since,
    CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    return await db.Set<UserNotificationEntity>()
      .AsNoTracking()
      .Where(n => n.UserWallet == userWallet && n.CreatedAt > since)
      .OrderByDescending(n => n.CreatedAt)
      .Select(n => ToDto(n))
      .ToListAsync(ct);
  }

  private static UserNotificationDto ToDto(UserNotificationEntity entity)
  {
    return new UserNotificationDto(
      entity.Id,
      entity.UserWallet,
      entity.Title,
      entity.Message,
      entity.NotificationType,
      entity.RelatedEntityId,
      entity.IsRead,
      entity.CreatedAt,
      entity.ReadAt,
      entity.Metadata
    );
  }
}