namespace Domain.Models.Dtos;

public sealed class PaymentMethodDto
{
  public int Id { get; set; }
  public string Code { get; set; } = default!;
  public string Name { get; set; } = default!;
  public PaymentCategoryDto? Category { get; set; }
}