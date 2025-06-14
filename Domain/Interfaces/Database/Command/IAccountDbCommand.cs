namespace Domain.Interfaces.Database.Command;

public interface IAccountDbCommand
{
    Task UpsertAccountAsync(string accountWallet);
    Task UpdateAccountInfoAsync(string token, long id, string userName);

}