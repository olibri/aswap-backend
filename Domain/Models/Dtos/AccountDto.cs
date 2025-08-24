using Domain.Models.DB;

namespace Domain.Models.Dtos;

public class AccountDto 
{
    public string WalletAddress { get; set; }
    public string? Telegram { get; set; }
    public string? TelegramId { get; set; }
    public int? OrdersCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastActiveTime { get; set; }

    public static AccountDto ToDto(AccountEntity entity)
    {
        return new AccountDto
        {
            WalletAddress = entity.WalletAddress,
            Telegram = entity.Telegram,
            TelegramId = entity.TelegramId,
            OrdersCount = entity.OrdersCount,
            CreatedAtUtc = entity.CreatedAtUtc,
            LastActiveTime = entity.LastActiveTime
        };
    }
}