using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Services.Account;

namespace Tests_back.Extensions;

public static class AccountExtention
{
    public static string GenerateFakeUser()
    {
        return "test_user_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    public static async Task SaveFakeUserToDbAsync(string userName, IAccountService accountService)
    {
        await accountService.CreateAccountWithReferralAsync(userName);
    }

    public static async Task UpdateFakeUserAsync(string token, long id, string userName,
        IAccountDbCommand accountDbCommand)
    {
        await accountDbCommand.UpdateAccountInfoAsync(token, id, userName);
    }
}