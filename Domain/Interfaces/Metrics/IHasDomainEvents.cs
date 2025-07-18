using Domain.Models;

namespace Domain.Interfaces.Metrics;

public interface IHasDomainEvents
{
  List<DomainEvent> DomainEvents { get; }
}