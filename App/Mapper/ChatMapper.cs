using Domain.Models.DB;
using Domain.Models.Dtos;
using Riok.Mapperly.Abstractions;

namespace App.Mapper;

[Mapper]
public static partial class ChatMapper
{
    [MapProperty(nameof(MessageDto.DealId), nameof(MessageEntity.RoomDealId))]
    [MapProperty(nameof(MessageDto.AccountId), nameof(MessageEntity.AccountId))]
    [MapProperty(nameof(MessageDto.Content), nameof(MessageEntity.Content))]
    [MapProperty(nameof(MessageDto.CreatedAtUtc), nameof(MessageEntity.CreatedAtUtc))]
    public static partial MessageEntity ToEntity(MessageDto dto);

    private static void OnAfterToEntity(MessageDto src, MessageEntity dest)
    {
        if (dest.CreatedAtUtc == default)
            dest.CreatedAtUtc = DateTime.UtcNow;

    }
}