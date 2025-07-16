using Domain.Models.Enums;

namespace Domain.Models.Dtos;

public record OfferCreated(Guid Id, Guid OrderId, ulong DealId,
  string Wallet, OrderSide Side)
  : DomainEvent(Id, DateTime.UtcNow);