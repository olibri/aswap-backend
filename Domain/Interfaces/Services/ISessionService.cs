using Domain.Models.Api;

namespace Domain.Interfaces.Services;

public interface ISessionService
{
  Task StartAsync(SessionPingDto dto, CancellationToken ct);
  Task PingAsync(SessionPingDto dto, CancellationToken ct);
}