using Domain.Models.Dtos;

namespace Domain.Interfaces.Chat;

public interface IChatDbCommand
{
    Task<Guid> CreateMessageAsync(MessageDto message);
    Task<MessageDto[]> GetMessagesAsync(ulong roomId);

    Task<string> GenerateCode(string wallet);
}