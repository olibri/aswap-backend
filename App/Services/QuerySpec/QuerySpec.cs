using Domain.Interfaces.QuerySpecs;
using Domain.Models.Api.QuerySpecs;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace App.Services.QuerySpec;

public class QuerySpec<T>
{
  private readonly List<IFilterRule<T>> _filters = new();
  private readonly List<ISortRule<T>> _sorts = new();
  private PageSpec _page = new();

  public QuerySpec<T> Where(IFilterRule<T> rule)
  {
    _filters.Add(rule);
    return this;
  }

  public QuerySpec<T> OrderBy(ISortRule<T> rule)
  {
    _sorts.Add(rule);
    return this;
  }

  public QuerySpec<T> Page(int page, int size)
  {
    _page = new PageSpec(page, size);
    return this;
  }
  private static void Log(string tag, IQueryable<T> q)
  {
    try { Debug.WriteLine($"-- {tag} --\n{q.ToQueryString()}"); } catch { }
  }

  public async Task<PagedResult<T>> ExecuteAsync(IQueryable<T> source)
  {
    //Log("START", source);

    foreach (var f in _filters)
    {
      source = f.Apply(source);
      //Log($"AFTER {f.GetType().Name}", source);
    }

    if (_sorts.Count > 0)
    {
      IOrderedQueryable<T>? ordered = null;
      foreach (var s in _sorts)
      {
        ordered = ordered is null ? s.ApplyFirst(source) : s.ApplyNext(ordered);
        //Log($"AFTER SORT {s.GetType().Name}", ordered!);
      }
      source = ordered!;
    }

    var total = await source.CountAsync();
    var pageData = await source.Skip(_page.Skip).Take(_page.Size).ToListAsync();
    return new PagedResult<T>(pageData, _page.Number, _page.Size, total);
  }
}