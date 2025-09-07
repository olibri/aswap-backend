using Domain.Models.Api.Order;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Services.Order;

public interface IChildOffersService
{
  Task<ChildOrderDto> UpsertAsync(ChildOrderUpsertDto dto, CancellationToken ct = default);

  Task<IReadOnlyList<ChildOrderDto>> GetByParentAsync(long dealId, CancellationToken ct = default);

}