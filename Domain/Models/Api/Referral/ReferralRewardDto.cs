namespace Domain.Models.Api.Referral;

public class ReferralRewardDto
{
  public Guid Id { get; set; }
  public string RefereeWallet { get; set; }
  public Guid OrderId { get; set; }
  public decimal RewardUsd { get; set; }
  public decimal RewardPercentage { get; set; }
  public decimal OrderValueUsd { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? ProcessedAt { get; set; }
}