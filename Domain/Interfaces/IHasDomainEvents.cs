using Domain.Models;

namespace Domain.Interfaces;

public interface IHasDomainEvents
{
  List<DomainEvent> DomainEvents { get; }
}