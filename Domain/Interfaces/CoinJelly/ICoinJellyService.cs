using Domain.Models.Api.CoinJelly;

namespace Domain.Interfaces.CoinJelly;

public interface ICoinJellyService
{
  Task<Guid> AddNewCoinJellyMethod(CoinJellyDto dto, CancellationToken ct);

  Task<bool> DeleteCoinJellyMethod(Guid id, CancellationToken ct);
  Task<CoinJellyAccountHistoryRequest[]> GetUserJellyHistoryAsync(string userWallet, CancellationToken ct);
  Task<CoinJellyAccountHistoryRequest> CreateNewJellyAsync(NewUserCoinJellyRequest dto, CancellationToken ct);

  Task<CoinJellyAccountHistoryRequest> UpdateJellyAsync(CoinJellyUpdateRequest req, CancellationToken ct);

  Task<CoinJellyDto[]> GetAllJellyMethodsAsync(CancellationToken ct);
}