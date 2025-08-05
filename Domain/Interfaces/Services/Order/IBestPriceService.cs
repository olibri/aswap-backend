using Domain.Models.Api.Order;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Services.Order;

public interface IBestPriceService
{
  Task<BestPriceDto?> GetBestPriceAsync(BestPriceRequest req, CancellationToken ct);
}