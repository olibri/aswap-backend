using Domain.Enums;

namespace Domain.Models.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public OrderTypes OrderType { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
    public string UserWallet { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}