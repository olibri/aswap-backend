namespace Domain.Models.Api.Referral;

public class ReferredUserDto
{
  public string WalletAddress { get; set; }
  public DateTime JoinedAt { get; set; }
  public int OrdersCount { get; set; }
  public decimal TotalVolumeUsd { get; set; }
  public decimal EarnedFromThisUserUsd { get; set; }
}