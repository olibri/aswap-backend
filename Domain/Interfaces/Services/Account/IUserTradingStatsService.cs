using Domain.Models.Api.User;

namespace Domain.Interfaces.Services.Account;

public interface IUserTradingStatsService
{
  Task<UserTradingStatsDto> GetUserStatsAsync(string userWallet, CancellationToken ct = default);
}