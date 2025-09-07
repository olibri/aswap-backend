namespace Domain.Models.Dtos;

public sealed class EscrowOrderWithChildrenDto
{
  public EscrowOrderDto Parent { get; init; } = default!;
  public IReadOnlyList<ChildOrderDto> Children { get; init; } = Array.Empty<ChildOrderDto>();
}