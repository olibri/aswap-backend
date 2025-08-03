namespace Domain.Interfaces.Services.IP;

public interface IGeoIpService
{
  string? ResolveCountry(string ip);
}