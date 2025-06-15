using Domain.Models.Dtos;

namespace Domain.Interfaces.Database.Queries;

public interface IAccountDbQueries
{
    Task<AccountDto?> GetAccountByWalletAsync(string wallet);
    Task<List<long>> GetChatIdsAsync(IEnumerable<string> wallets);
}