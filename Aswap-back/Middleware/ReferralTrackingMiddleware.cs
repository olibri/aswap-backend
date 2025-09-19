using Domain.Interfaces.Services.Account;

namespace Aswap_back.Middleware;

public class ReferralTrackingMiddleware(RequestDelegate next, ILogger<ReferralTrackingMiddleware> logger)
{
  public async Task InvokeAsync(HttpContext context, IReferralService referralService)
  {
    // Check if this is a referral link access
    if (context.Request.Path.StartsWithSegments("/referral") && context.Request.Path.HasValue)
    {
      var pathSegments = context.Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
      if (pathSegments.Length >= 2)
      {
        var referralCode = pathSegments[1];

        // Store referral code in session/cookie for later use when user registers
        context.Response.Cookies.Append("referral_code", referralCode, new CookieOptions
        {
          MaxAge = TimeSpan.FromDays(30),
          HttpOnly = true,
          Secure = context.Request.IsHttps,
          SameSite = SameSiteMode.Lax
        });

        logger.LogInformation("Referral code {ReferralCode} tracked for session", referralCode);

        // Redirect to main app
        context.Response.Redirect("/");
        return;
      }
    }

    await next(context);
  }
}