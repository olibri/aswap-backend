using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models.DB;

[Table("rooms")]
[Index(nameof(DealId), IsUnique = true)]
public class RoomEntity
{
    [Key][Column("deal_id")] public ulong DealId { get; set; }
    [Column("last_message_time")] public DateTime LastMessageTime { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }

    public ICollection<MessageEntity> Messages { get; set; }
}