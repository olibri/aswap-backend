using Domain.Enums;

namespace Domain.Models.Dtos;

public record CreateOrderDto(
    OrderTypes OrderType,
    string Currency,
    decimal Amount,
    decimal Price,
    string UserId
);