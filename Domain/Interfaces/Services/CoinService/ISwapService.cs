using Domain.Models.Api.QuerySpecs;
using Domain.Models.Api.Swap;
using Domain.Models.Dtos;
using Domain.Models.Dtos.Jupiter;

namespace Domain.Interfaces.Services.CoinService;

public interface ISwapService
{
  Task<SwapResponseDto> AddSwapAsync(SwapBuildRequestDto body, CancellationToken ct);
  public Task<PagedResult<AccountSwapHistoryDto>> SwapHistoryAsync(SwapHistoryQuery q, CancellationToken ct);
}