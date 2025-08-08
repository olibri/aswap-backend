namespace Domain.Interfaces.QuerySpecs;

public interface IFilterRule<T>
{
  IQueryable<T> Apply(IQueryable<T> query);
}