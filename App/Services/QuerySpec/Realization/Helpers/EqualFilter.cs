using System.Linq.Expressions;
using Domain.Interfaces.QuerySpecs;

public sealed class EqualFilter<T, TProp> : IFilterRule<T>
{
  private readonly Expression<Func<T, bool>> _predicate;

  public EqualFilter(Expression<Func<T, TProp>> selector, TProp value, bool skipIfNull = true)
  {
    if (skipIfNull && value is null)
    {
      _predicate = _ => true;
      return;
    }

    var p = Expression.Parameter(typeof(T), "e");

    var left0 = selector.Body is UnaryExpression u && u.NodeType == ExpressionType.Convert ? u.Operand : selector.Body;
    var left = new Replace(selector.Parameters[0], p).Visit(left0)!;

    Expression right;

    if (left.Type.IsEnum)
    {
      var utype = Enum.GetUnderlyingType(left.Type);
      left = Expression.Convert(left, utype);
      var boxed = Convert.ChangeType(value, utype);
      right = Expression.Constant(boxed, utype);
    }
    else
    {
      right = Expression.Constant(value, typeof(TProp));
      if (left.Type != right.Type)
        right = Expression.Convert(right, left.Type);
    }

    var body = Expression.Equal(left, right);
    _predicate = Expression.Lambda<Func<T, bool>>(body, p);
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