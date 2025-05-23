using Domain.Enums;

namespace Domain.Models.Dtos;

public class UpdateOrderDto
{
    public ulong OrderId { get; set; }
    public decimal MinFiatAmount { get; set; }
    public decimal MaxFiatAmount { get; set; }
}