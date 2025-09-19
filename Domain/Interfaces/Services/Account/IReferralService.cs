using Domain.Models.Api.Referral;

namespace Domain.Interfaces.Services.Account;

public interface IReferralService
{
  Task<string> GenerateReferralCodeAsync(string walletAddress, CancellationToken ct = default);
  Task<string> GetReferralLinkAsync(string walletAddress, CancellationToken ct = default);
  Task<bool> ProcessReferralAsync(string referralCode, string newUserWallet, CancellationToken ct = default);
  Task<ReferralStatsDto> GetReferralStatsAsync(string walletAddress, CancellationToken ct = default);

  Task<List<ReferredUserDto>> GetReferredUsersAsync(string walletAddress, int page = 1, int pageSize = 20,
    CancellationToken ct = default);

  Task<bool> ValidateReferralCodeAsync(string referralCode, CancellationToken ct = default);
}