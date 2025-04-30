using Domain.Enums;

namespace Domain.Models.Dtos;

public record CreateOrderDto(
    string EscrowPda,

    OrderTypes OrderType,
    string Currency,
    decimal Amount,
    decimal Price,
    string UserId
);