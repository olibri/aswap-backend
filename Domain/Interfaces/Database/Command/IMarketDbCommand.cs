using Domain.Models.Dtos;

namespace Domain.Interfaces.Database.Command;

public interface IMarketDbCommand
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto order);
}