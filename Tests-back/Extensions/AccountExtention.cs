using Domain.Interfaces.Chat;

namespace Tests_back.Extensions;

public static class AccountExtention
{
    public static string GenerateFakeUser()
    {
        return "test_user_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    public static async Task SaveFakeUserToDbAsync(string userName, IChatDbCommand chatDbCommand)
    {
        await chatDbCommand.UpsertAccountAsync(userName);
    }

}