using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("telegram_link")]
public class TelegramLinkEntity
{
    [Key]
    [Column("code")]
    public string Code { get; set; }

    [Column("wallet_address")]
    [Required]
    public string WalletAddress { get; set; }

    [Column("expired_at")]
    [Required]
    public DateTime ExpiredAt { get; set; }
}