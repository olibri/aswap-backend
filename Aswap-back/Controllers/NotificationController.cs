using Domain.Interfaces.Services.Notification;
using Domain.Models.Api.QuerySpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController(INotificationService notificationService) : Controller
{
  [HttpGet]
  public async Task<IActionResult> GetNotifications([FromQuery] NotificationQuery query)
  {
    var userWallet = GetUserWallet();
    var result = await notificationService.GetUserNotificationsAsync(userWallet, query);
    return Ok(result);
  }

  [HttpGet("unread-count")]
  public async Task<IActionResult> GetUnreadCount()
  {
    var userWallet = GetUserWallet();
    var count = await notificationService.GetUnreadCountAsync(userWallet);
    return Ok(new { count });
  }

  [HttpPut("{id}/read")]
  public async Task<IActionResult> MarkAsRead(Guid id)
  {
    await notificationService.MarkAsReadAsync(id);
    return NoContent();
  }

  [HttpPut("read-all")]
  public async Task<IActionResult> MarkAllAsRead()
  {
    var userWallet = GetUserWallet();
    await notificationService.MarkAllAsReadAsync(userWallet);
    return NoContent();
  }

  [HttpGet("realtime")]
  public async Task<IActionResult> GetRealtimeNotifications([FromQuery] DateTime since)
  {
    var userWallet = GetUserWallet();
    var notifications = await notificationService.GetRealtimeNotificationsAsync(userWallet, since);
    return Ok(notifications);
  }

  private string GetUserWallet()
  {
    return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? throw new UnauthorizedAccessException("User wallet not found in token");
  }
}