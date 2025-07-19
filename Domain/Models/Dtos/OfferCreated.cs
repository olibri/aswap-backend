using Domain.Enums;
using Domain.Models.Enums;

namespace Domain.Models.Dtos;

public record OfferCreated(Guid Id, Guid OrderId, ulong DealId,
  string Wallet, OrderSide Side, EventType Type)
  : DomainEvent(Id, DateTime.UtcNow, Type);