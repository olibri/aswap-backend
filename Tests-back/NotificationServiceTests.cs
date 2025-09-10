using Domain.Enums;
using Domain.Interfaces.Services.Notification;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class NotificationServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  private readonly INotificationService _notificationService = fixture.GetService<INotificationService>();

  [Fact]
  public async Task CreateNotificationAsync_Should_Create_And_Return_Notification()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_1");

    var title = "Test Notification";
    var message = "This is a test notification message";
    var type = NotificationType.OrderStatusChanged;
    var relatedEntityId = "order_123";
    var metadata = """{"orderId": "123", "status": "completed"}""";

    // Act
    var result = await _notificationService.CreateNotificationAsync(
        "test_wallet_1",
        title,
        message,
        type,
        relatedEntityId,
        metadata);

    // Assert
    result.ShouldNotBeNull();
    result.UserWallet.ShouldBe("test_wallet_1");
    result.Title.ShouldBe(title);
    result.Message.ShouldBe(message);
    result.NotificationType.ShouldBe(type);
    result.RelatedEntityId.ShouldBe(relatedEntityId);
    result.Metadata.ShouldBe(metadata);
    result.IsRead.ShouldBeFalse();
    result.ReadAt.ShouldBeNull();
    result.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);

    // Verify in database
    var dbNotification = await db.Set<UserNotificationEntity>()
        .FirstOrDefaultAsync(n => n.Id == result.Id);
    dbNotification.ShouldNotBeNull();
    dbNotification!.Title.ShouldBe(title);
  }

  [Fact]
  public async Task GetUserNotificationsAsync_Should_Return_Paginated_Results()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_2");

    // Create 15 notifications
    for (int i = 1; i <= 15; i++)
    {
      await _notificationService.CreateNotificationAsync(
          "test_wallet_2",
          $"Title {i}",
          $"Message {i}",
          NotificationType.SystemMessage);
    }

    var query = new NotificationQuery { Number = 1, Size = 10 };

    // Act
    var result = await _notificationService.GetUserNotificationsAsync("test_wallet_2", query);

    // Assert
    result.ShouldNotBeNull();
    result.Data.Count.ShouldBe(10);
    result.Total.ShouldBe(15);
    result.Page.ShouldBe(1);
    result.Size.ShouldBe(10);

    // Should be ordered by CreatedAt descending
    result.Data[0].Title.ShouldBe("Title 15");
    result.Data[9].Title.ShouldBe("Title 6");
  }

  [Fact]
  public async Task GetUserNotificationsAsync_Should_Filter_By_IsRead()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_3");

    // Create 5 unread and 3 read notifications
    var notifications = new List<UserNotificationDto>();
    for (int i = 1; i <= 8; i++)
    {
      var notification = await _notificationService.CreateNotificationAsync(
          "test_wallet_3",
          $"Title {i}",
          $"Message {i}",
          NotificationType.TradeCompleted);
      notifications.Add(notification);
    }

    // Mark first 3 as read
    for (int i = 0; i < 3; i++)
    {
      await _notificationService.MarkAsReadAsync(notifications[i].Id);
    }

    var queryUnread = new NotificationQuery { IsRead = false };
    var queryRead = new NotificationQuery { IsRead = true };

    // Act
    var unreadResult = await _notificationService.GetUserNotificationsAsync("test_wallet_3", queryUnread);
    var readResult = await _notificationService.GetUserNotificationsAsync("test_wallet_3", queryRead);

    // Assert
    unreadResult.Total.ShouldBe(5);
    readResult.Total.ShouldBe(3);

    unreadResult.Data.All(n => !n.IsRead).ShouldBeTrue();
    readResult.Data.All(n => n.IsRead).ShouldBeTrue();
    readResult.Data.All(n => n.ReadAt.HasValue).ShouldBeTrue();
  }

  [Fact]
  public async Task GetUserNotificationsAsync_Should_Filter_By_Date_Range()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_4");

    var yesterday = DateTime.UtcNow.AddDays(-1);
    var tomorrow = DateTime.UtcNow.AddDays(1);

    // Create notifications with different dates by manipulating database directly
    var oldNotification = new UserNotificationEntity
    {
      UserWallet = "test_wallet_4",
      Title = "Old Notification",
      Message = "This is old",
      NotificationType = NotificationType.SystemMessage,
      CreatedAt = yesterday
    };

    var recentNotification = new UserNotificationEntity
    {
      UserWallet = "test_wallet_4",
      Title = "Recent Notification",
      Message = "This is recent",
      NotificationType = NotificationType.SystemMessage,
      CreatedAt = DateTime.UtcNow
    };

    db.Set<UserNotificationEntity>().AddRange(oldNotification, recentNotification);
    await db.SaveChangesAsync();

    var query = new NotificationQuery
    {
      FromDate = DateTime.UtcNow.AddHours(-1),
      ToDate = tomorrow
    };

    // Act
    var result = await _notificationService.GetUserNotificationsAsync("test_wallet_4", query);

    // Assert
    result.Total.ShouldBe(1);
    result.Data[0].Title.ShouldBe("Recent Notification");
  }

  [Fact]
  public async Task MarkAsReadAsync_Should_Update_Notification()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_5");

    var notification = await _notificationService.CreateNotificationAsync(
        "test_wallet_5",
        "Test Title",
        "Test Message",
        NotificationType.PaymentReceived);

    notification.IsRead.ShouldBeFalse();

    // Act
    await _notificationService.MarkAsReadAsync(notification.Id);

    // Assert
    var updated = await db.Set<UserNotificationEntity>()
        .AsNoTracking()
        .FirstAsync(n => n.Id == notification.Id);

    updated.IsRead.ShouldBeTrue();
    updated.ReadAt.ShouldNotBeNull();
    updated.ReadAt!.Value.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
  }

  [Fact]
  public async Task MarkAsReadAsync_Should_Throw_When_Notification_Not_Found()
  {
    // Act & Assert
    var nonExistentId = Guid.NewGuid();

    await Should.ThrowAsync<InvalidOperationException>(
        () => _notificationService.MarkAsReadAsync(nonExistentId));
  }

  [Fact]
  public async Task MarkAllAsReadAsync_Should_Update_All_Unread_Notifications()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_6");

    // Create 5 notifications for user
    for (int i = 1; i <= 5; i++)
    {
      await _notificationService.CreateNotificationAsync(
          "test_wallet_6",
          $"Title {i}",
          $"Message {i}",
          NotificationType.DisputeOpened);
    }

    // Create 2 notifications for another user (should not be affected)
    await SeedUserAsync(db, "other_wallet");
    for (int i = 1; i <= 2; i++)
    {
      await _notificationService.CreateNotificationAsync(
          "other_wallet",
          $"Other Title {i}",
          $"Other Message {i}",
          NotificationType.SystemMessage);
    }

    // Act
    await _notificationService.MarkAllAsReadAsync("test_wallet_6");

    // Assert
    var userNotifications = await db.Set<UserNotificationEntity>()
        .AsNoTracking()
        .Where(n => n.UserWallet == "test_wallet_6")
        .ToListAsync();

    var otherUserNotifications = await db.Set<UserNotificationEntity>()
        .AsNoTracking()
        .Where(n => n.UserWallet == "other_wallet")
        .ToListAsync();

    userNotifications.All(n => n.IsRead).ShouldBeTrue();
    userNotifications.All(n => n.ReadAt.HasValue).ShouldBeTrue();

    otherUserNotifications.All(n => !n.IsRead).ShouldBeTrue();
    otherUserNotifications.All(n => !n.ReadAt.HasValue).ShouldBeTrue();
  }

  [Fact]
  public async Task GetUnreadCountAsync_Should_Return_Correct_Count()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_7");

    // Create 7 notifications
    var notifications = new List<UserNotificationDto>();
    for (int i = 1; i <= 7; i++)
    {
      var notification = await _notificationService.CreateNotificationAsync(
          "test_wallet_7",
          $"Title {i}",
          $"Message {i}",
          NotificationType.PromotionalOffer);
      notifications.Add(notification);
    }

    // Mark 3 as read
    for (int i = 0; i < 3; i++)
    {
      await _notificationService.MarkAsReadAsync(notifications[i].Id);
    }

    // Act
    var unreadCount = await _notificationService.GetUnreadCountAsync("test_wallet_7");

    // Assert
    unreadCount.ShouldBe(4);
  }

  [Fact]
  public async Task GetRealtimeNotificationsAsync_Should_Return_Notifications_After_Date()
  {
    // Arrange
    fixture.ResetDb("user_notifications", "account");
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await SeedUserAsync(db, "test_wallet_8");

    var cutoffTime = DateTime.UtcNow.AddMinutes(-5);

    // Create older notification by direct database insert
    var oldNotification = new UserNotificationEntity
    {
      UserWallet = "test_wallet_8",
      Title = "Old Notification",
      Message = "This is old",
      NotificationType = NotificationType.SystemMessage,
      CreatedAt = cutoffTime.AddMinutes(-1)
    };

    db.Set<UserNotificationEntity>().Add(oldNotification);
    await db.SaveChangesAsync();

    // Create recent notifications through service
    await _notificationService.CreateNotificationAsync(
        "test_wallet_8",
        "Recent Notification 1",
        "This is recent 1",
        NotificationType.OrderStatusChanged);

    await _notificationService.CreateNotificationAsync(
        "test_wallet_8",
        "Recent Notification 2",
        "This is recent 2",
        NotificationType.TradeCompleted);

    // Act
    var realtimeNotifications = await _notificationService.GetRealtimeNotificationsAsync(
        "test_wallet_8",
        cutoffTime);

    // Assert
    var notificationsList = realtimeNotifications.ToList();
    notificationsList.Count.ShouldBe(2);
    notificationsList.All(n => n.CreatedAt > cutoffTime).ShouldBeTrue();
    notificationsList.Any(n => n.Title == "Old Notification").ShouldBeFalse();
  }

  [Fact]
  public async Task GetUserNotificationsAsync_Should_Return_Empty_For_Nonexistent_User()
  {
    // Arrange
    var query = new NotificationQuery();

    // Act
    var result = await _notificationService.GetUserNotificationsAsync("nonexistent_wallet", query);

    // Assert
    result.ShouldNotBeNull();
    result.Data.ShouldBeEmpty();
    result.Total.ShouldBe(0);
  }

  private static async Task SeedUserAsync(P2PDbContext db, string walletAddress)
  {
    if (!await db.Account.AnyAsync(a => a.WalletAddress == walletAddress))
    {
      db.Account.Add(new AccountEntity
      {
        WalletAddress = walletAddress,
        CreatedAtUtc = DateTime.UtcNow,
        LastActiveTime = DateTime.UtcNow
      });
      await db.SaveChangesAsync();
    }
  }
}