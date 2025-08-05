using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Tests_back.Extensions.Order;

public static class MvcResultExt
{
  public static T? OkValue<T>(this IActionResult result)
  {
    result.ShouldBeOfType<OkObjectResult>();
    var ok = (OkObjectResult)result;
    return (T?)ok.Value;
  }

  public static async Task<T?> OkValueAsync<T>(this Task<IActionResult> task)
    => (await task).OkValue<T>();
}