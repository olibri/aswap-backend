using Domain.Models.Api.CoinPrice;

namespace Domain.Interfaces.Services.CoinService.TokenRepo;

public interface ITokenRepository
{
  Task<bool> AnyAsync(CancellationToken ct);
  Task UpsertAsync(IEnumerable<TokenDto> tokens, CancellationToken ct);

  Task<IReadOnlyList<string>> GetAllMintsAsync(CancellationToken ct);

  Task<TokenDto?> GetByMintAsync(string mint, CancellationToken ct);
}