using Domain.Interfaces.QuerySpecs;
using System.Linq.Expressions;

namespace App.Services.QuerySpec.Realization.Helpers;

public sealed class AnyInCollectionFilter<T, TItem, TValue> : IFilterRule<T>
{
  private readonly Expression<Func<T, bool>> _predicate;

  public AnyInCollectionFilter(
    Expression<Func<T, IEnumerable<TItem>>> collection,
    Expression<Func<TItem, TValue>> selector,
    IEnumerable<TValue> values,
    bool skipIfEmpty = true)
  {
    var vals = values?.ToArray() ?? [];
    if (skipIfEmpty && vals.Length == 0)
    {
      _predicate = _ => true;
      return;
    }

    var param = Expression.Parameter(typeof(T), "e");
    var colExpr = new Replace(collection.Parameters[0], param).Visit(collection.Body)!;

    var itemParam = Expression.Parameter(typeof(TItem), "x");
    var selBody = new Replace(selector.Parameters[0], itemParam).Visit(selector.Body)!;

    var contains = typeof(Enumerable).GetMethods()
      .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
      .MakeGenericMethod(typeof(TValue));
    var valuesConst = Expression.Constant(vals);

    var innerBody = Expression.Call(contains, valuesConst, selBody);

    var any = typeof(Enumerable).GetMethods()
      .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
      .MakeGenericMethod(typeof(TItem));
    var lambda = Expression.Lambda(innerBody, itemParam);
    var anyCall = Expression.Call(any, colExpr, lambda);

    _predicate = Expression.Lambda<Func<T, bool>>(anyCall, param);
  }

  public IQueryable<T> Apply(IQueryable<T> source)
  {
    return source.Where(_predicate);
  }

  private sealed class Replace(Expression from, Expression to) : ExpressionVisitor
  {
    public override Expression? Visit(Expression? node)
    {
      return node == from ? to : base.Visit(node);
    }
  }
}