namespace Domain.Models.Api.Referral;

public class ReferralStatsDto
{
  public string WalletAddress { get; set; }
  public string ReferralCode { get; set; }
  public string ReferralLink { get; set; }
  public int TotalReferrals { get; set; }
  public decimal TotalEarningsUsd { get; set; }
  public int PendingRewards { get; set; }
  public decimal PendingRewardsUsd { get; set; }
  public List<ReferralRewardDto> RecentRewards { get; set; } = new();
}