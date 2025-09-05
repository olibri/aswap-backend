using Domain.Models.Dtos.Jupiter;

namespace Domain.Interfaces.Services.CoinService;

public interface ISwapService
{
  Task<SwapResponseDto> AddSwapAsync(SwapBuildRequestDto body, CancellationToken ct);
}