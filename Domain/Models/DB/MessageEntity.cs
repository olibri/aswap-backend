using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("messages")]
[Index(nameof(RoomDealId), nameof(CreatedAtUtc))]
public class MessageEntity
{
    [Key] 
    [Column("id")] 
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] 
    [Column("room_id")] 
    public ulong RoomDealId { get; set; }

    [Required] 
    [Column("account_id")]
    public string AccountId { get; set; }

    [Column("content")] public string Content { get; set; }
    
    [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }


    [ForeignKey(nameof(RoomDealId))]
    public RoomEntity Room { get; set; }

    [ForeignKey(nameof(AccountId))]
    public AccountEntity Account { get; set; }

}