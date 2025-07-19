using Domain.Enums;

namespace Domain.Models;

public abstract record DomainEvent(Guid Id, DateTime OccurredAt, EventType Type);