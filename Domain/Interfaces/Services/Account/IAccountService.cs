using Domain.Models.Api.Auth;
using Domain.Models.DB;

namespace Domain.Interfaces.Services.Account;

public interface IAccountService
{
  Task TouchAsync(string wallet, CancellationToken ct);
  Task<DateTime?> GetBanUntilAsync(string wallet, CancellationToken ct);

  Task<long> BanAsync(BanUserDto banUserDto, CancellationToken ct);

  public Task<AccountEntity> CreateAccountWithReferralAsync(string walletAddress, string? referralCode = null,
    CancellationToken ct = default);
}