using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("referral_rewards")]
public class ReferralRewardEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("referrer_wallet")]
    public string ReferrerWallet { get; set; }

    [Required]
    [Column("referee_wallet")]
    public string RefereeWallet { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("reward_usd", TypeName = "decimal(18,6)")]
    public decimal RewardUsd { get; set; }

    [Required]
    [Column("reward_percentage", TypeName = "decimal(5,2)")]
    public decimal RewardPercentage { get; set; }

    [Required]
    [Column("order_value_usd", TypeName = "decimal(18,6)")]
    public decimal OrderValueUsd { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ReferrerWallet))]
    public AccountEntity InviterAccount { get; set; }  

    [ForeignKey(nameof(RefereeWallet))]
    public AccountEntity InvitedAccount { get; set; }  

    [ForeignKey(nameof(OrderId))]
    public EscrowOrderEntity Order { get; set; }
}