using System.Security.Claims;

namespace App.Utils;

public static class UserWallet
{
  public static string GetUserWallet(this ClaimsPrincipal principal)
  {
    var wallet = principal.FindFirst("sub")?.Value
                 ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(wallet))
      throw new UnauthorizedAccessException("User wallet not found in token");

    return wallet;
  }
}