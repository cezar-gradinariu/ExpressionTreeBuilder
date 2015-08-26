using System;
using System.Linq;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, right.WithParametersOf(left).Body),
                left.Parameters);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, right.WithParametersOf(left).Body),
                left.Parameters);
        }

        private static Expression<Func<TResult>> WithParametersOf<T, TResult>(this Expression<Func<T, TResult>> left,
            Expression<Func<T, TResult>> right)
        {
            return new ReplaceParameterVisitor<Func<TResult>>(left.Parameters[0], right.Parameters[0]).Visit(left);
        }

        public class ReplaceParameterVisitor<TResult> : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;
            private readonly Expression _replacement;

            public ReplaceParameterVisitor(ParameterExpression parameter, Expression replacement)
            {
                _parameter = parameter;
                _replacement = replacement;
            }

            public Expression<TResult> Visit<T>(Expression<T> node)
            {
                var parameters = node.Parameters.Where(p => p != _parameter);
                return Expression.Lambda<TResult>(Visit(node.Body), parameters);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _parameter ? _replacement : base.VisitParameter(node);
            }
        }
    }
}