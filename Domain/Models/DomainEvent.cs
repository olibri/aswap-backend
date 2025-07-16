namespace Domain.Models;

public abstract record DomainEvent(Guid Id, DateTime OccurredAt);