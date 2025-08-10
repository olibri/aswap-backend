using System.Linq.Expressions;
using Domain.Interfaces.QuerySpecs;

namespace App.Services.QuerySpec.Realization.Helpers;

public sealed class DateRangeFilter<T> : IFilterRule<T>
{
  private readonly Expression<Func<T, bool>> _predicate;

  public DateRangeFilter(Expression<Func<T, DateTime>> selector, DateTime from, DateTime to)
  {
    var p = Expression.Parameter(typeof(T), "e");
    var left0 = selector.Body is UnaryExpression u && u.NodeType == ExpressionType.Convert ? u.Operand : selector.Body;
    var left = new Replace(selector.Parameters[0], p).Visit(left0)!;

    var fromConst = Expression.Constant(from, typeof(DateTime));
    var toConst = Expression.Constant(to, typeof(DateTime));

    var ge = Expression.GreaterThanOrEqual(left, fromConst);
    var le = Expression.LessThanOrEqual(left, toConst);
    var and = Expression.AndAlso(ge, le);

    _predicate = Expression.Lambda<Func<T, bool>>(and, p);
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