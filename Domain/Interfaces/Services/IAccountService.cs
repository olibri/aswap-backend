using Domain.Models.Api.Auth;

namespace Domain.Interfaces.Services;

public interface IAccountService
{
  Task TouchAsync(string wallet, CancellationToken ct);
  Task<DateTime?> GetBanUntilAsync(string wallet, CancellationToken ct);

  Task<long> BanAsync(BanUserDto banUserDto, CancellationToken ct);

}