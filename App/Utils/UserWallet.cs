using Domain.Models.Api.Rating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace App.Utils;

public static class UserWallet
{
  //public static string GetUserWallet()
  //{
  //  var wallet = User.FindFirst("sub")?.Value
  //               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  //  if (string.IsNullOrEmpty(wallet)) throw new UnauthorizedAccessException("User wallet not found in token");

  //  return wallet;
  //}
}