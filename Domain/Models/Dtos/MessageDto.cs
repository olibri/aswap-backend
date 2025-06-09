using Domain.Models.DB;

namespace Domain.Models.Dtos;

public class MessageDto
{
    public ulong DealId { get; set; }
    public string AccountId { get; set; }
    public string Content { get; set; }

    public DateTime CreatedAtUtc { get; set; }


    public static MessageDto ToDto(MessageEntity entity)
    {
        return new MessageDto
        {
            DealId = entity.RoomDealId,
            AccountId = entity.AccountId,
            Content = entity.Content,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}