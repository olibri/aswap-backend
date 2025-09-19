namespace Domain.Models.Api.Referral;

public class ProcessReferralRequest
{
  public string ReferralCode { get; set; }
  public string NewUserWallet { get; set; }
}