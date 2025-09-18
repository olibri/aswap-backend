using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB.Metrics;

[Table("referral_stats_daily")]
public class ReferralStatsDailyEntity
{
    [Key]
    [Column("day")]
    public DateOnly Day { get; set; }

    [Key]
    [Column("referrer_wallet")]
    public string ReferrerWallet { get; set; }

    [Column("new_referrals")]
    public int NewReferrals { get; set; }

    [Column("total_referrals")]
    public int TotalReferrals { get; set; }

    [Column("rewards_earned_usd", TypeName = "decimal(18,6)")]
    public decimal RewardsEarnedUsd { get; set; }

    [Column("orders_by_referrals")]
    public int OrdersByReferrals { get; set; }

    [Column("volume_by_referrals_usd", TypeName = "decimal(18,6)")]
    public decimal VolumeByReferralsUsd { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(ReferrerWallet))]
    public AccountEntity InviterAccount { get; set; }
}