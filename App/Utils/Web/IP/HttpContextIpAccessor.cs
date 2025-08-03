using Domain.Interfaces.Services.IP;
using Microsoft.AspNetCore.Http;

namespace App.Utils.Web.IP;

public sealed class HttpContextIpAccessor(IHttpContextAccessor accessor) : IClientIpAccessor
{
  public string GetClientIp()
  {
    var ctx = accessor.HttpContext;
    var forwarded = ctx?.Request.Headers["X‑Forwarded‑For"].FirstOrDefault();
    return forwarded ?? ctx?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
  }
}