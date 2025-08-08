using System.Linq.Expressions;
using Domain.Interfaces.QuerySpecs;

namespace App.Services.QuerySpec.Realization.Helpers;

public sealed class SortRule<T>(Expression<Func<T, object>> prop, bool desc = false) : ISortRule<T>
{
  public IOrderedQueryable<T> ApplyFirst(IQueryable<T> src)
  {
    return desc
      ? src.OrderByDescending(prop)
      : src.OrderBy(prop);
  }

  public IOrderedQueryable<T> ApplyNext(IOrderedQueryable<T> src)
  {
    return desc
      ? src.ThenByDescending(prop)
      : src.ThenBy(prop);
  }
}