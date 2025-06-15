using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;

namespace Tests_back.Extensions;

public static class AccountExtention
{
    public static string GenerateFakeUser()
    {
        return "test_user_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    public static async Task SaveFakeUserToDbAsync(string userName, IAccountDbCommand accountDbCommand)
    {
        await accountDbCommand.UpsertAccountAsync(userName);
    }

    public static async Task UpdateFakeUserAsync(string token, long id, string userName,
        IAccountDbCommand accountDbCommand)
    {
        await accountDbCommand.UpdateAccountInfoAsync(token, id, userName);
    }
}