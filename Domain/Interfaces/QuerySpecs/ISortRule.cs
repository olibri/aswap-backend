namespace Domain.Interfaces.QuerySpecs;

public interface ISortRule<T>
{
  IOrderedQueryable<T> ApplyFirst(IQueryable<T> source);

  IOrderedQueryable<T> ApplyNext(IOrderedQueryable<T> source);
}