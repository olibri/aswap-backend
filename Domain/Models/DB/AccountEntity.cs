using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("account")]
public class AccountEntity
{
  [Key] [Column("wallet_address")] public string WalletAddress { get; set; }
  [Column("telegram")] public string? Telegram { get; set; }
  [Column("telegram_chat_id")] public string? TelegramId { get; set; }

  [Column("orders_count")] public int? OrdersCount { get; set; }
  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }
  [Column("last_active_time")] public DateTime? LastActiveTime { get; set; }

  [Column("referral_code")] public string? ReferralCode { get; set; }
  [Column("referred_by")] public string? ReferredBy { get; set; }
  [Column("referral_count")] public int ReferralCount { get; set; } = 0;
  [Column("referral_earnings_usd")] public decimal ReferralEarningsUsd { get; set; } = 0;

  public ICollection<MessageEntity> Messages { get; set; }

  [ForeignKey(nameof(ReferredBy))] public AccountEntity? MyInviter { get; set; }

  public ICollection<AccountEntity> MyInvited { get; set; } = new List<AccountEntity>();
  public ICollection<ReferralRewardEntity> EarnedRewards { get; set; } = new List<ReferralRewardEntity>();
}