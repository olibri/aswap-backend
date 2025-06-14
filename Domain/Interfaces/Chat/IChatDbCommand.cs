using Domain.Models.Dtos;

namespace Domain.Interfaces.Chat;

public interface IChatDbCommand
{
    Task<Guid> CreateMessageAsync(MessageDto message);
    Task<MessageDto[]> GetMessagesAsync(ulong roomId);

    Task UpdateAccountInfoAsync(string token, long id, string userName);

    Task<string> GenerateCode(string wallet);
    Task UpsertAccountAsync(string accountWallet);
}