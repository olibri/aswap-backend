using Domain.Interfaces.Services.Notification;
using Domain.Models.Api.QuerySpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Aswap_back.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotificationService notificationService) : Controller
{
  [HttpGet("test")]
  //[AllowAnonymous] // Тимчасово без авторизації
  public IActionResult Test()
  {
    var now = DateTime.UtcNow;
    Console.WriteLine($"Server UTC time: {now}");
    Console.WriteLine($"Unix timestamp: {((DateTimeOffset)now).ToUnixTimeSeconds()}");

    // Перевірте ваш токен
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.ReadJwtToken(
      "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1TnZxYVM4akRmWjMxeFRmYnpRd2c3WmFEdXRtVllqV0pjMnZLTHMzRmQxRSIsInJvbGUiOiJ1c2VyIiwibmJmIjoxNzU3NTczMDQ5LCJleHAiOjE3NTc1NzM5NDksImlzcyI6IkFzd2FwIiwiYXVkIjoiQXN3YXBBcHAifQ.imJYNlOKwDySxXlbRcjakp64npSkMXCQj3R6kovV72U");
    Console.WriteLine($"Token nbf: {token.Claims.FirstOrDefault(c => c.Type == "nbf")?.Value}");
    Console.WriteLine($"Token exp: {token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value}");

    var userWallet = GetUserWallet();
    Console.WriteLine($"userWallet: {userWallet}");

    return Ok(new { message = "Controller works without auth", time = DateTime.UtcNow });
  }

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
    // Спробувати різні claim types
    var wallet = User.FindFirst("http://schemas.xmlsoap.org/2003/ws/2005/05/identity/claims/nameidentifier")?.Value // ASP.NET mapped
                 ?? User.FindFirst("sub")?.Value // JWT standard
                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value // CLR constant
                 ?? User.Identity?.Name; // Fallback

    if (string.IsNullOrEmpty(wallet))
    {
      Console.WriteLine("=== DEBUG CLAIMS ===");
      foreach (var claim in User.Claims)
      {
        Console.WriteLine($"  {claim.Type} = {claim.Value}");
      }

      throw new UnauthorizedAccessException("User wallet not found in token");
    }

    return wallet;
  }
}