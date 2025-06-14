using Domain.Models.Dtos;

namespace Domain.Interfaces.Database.Queries;

public interface IAccountDbQueries
{
    Task<AccountDto?> GetAccountByWalletAsync(string wallet);
    Task<IEnumerable<long>> CheckIdAsync(string buyerWallet, string sellerWallet);
}