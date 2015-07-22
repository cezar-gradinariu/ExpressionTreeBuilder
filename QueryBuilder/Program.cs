using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryBuilder
{
    internal class Program
    {
        private static void Main()
        {
            var qObject = new QueryObject
            {
                QueryConditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        PropertyName = "Name",
                        Operation = "Contains",
                        Value = "na"
                    },
                    new QueryCondition
                    {
                        PropertyName = "Surname",
                        Operation = "Equal",
                        Value = "Gradinariu"
                    },
                    new QueryCondition
                    {
                        PropertyName = "Age",
                        Operation = "LessThanOrEqual",
                        Value = 10
                    }
                }
            };

            var list = new List<Person>
            {
                new Person {Name = "B", Surname = "G", Age = 30},
                new Person {Name = "A",Surname = "G1", Age = 35},
                new Person {Name = "Ana-Maria", Surname = "Gradinariu", Age = 3},
                new Person {Name = "Maya-Ioana",Surname = "Gradinariu", Age = 1},
            };
            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < 100000; i++)
            {
                //var s = list.Where(p => p.Name.Contains("ana")).ToList();
                var ex = qObject.BuildExpression<Person>().Compile();
                var s = list.Where(ex).ToList();
            }

            watch.Stop();
            var ms = watch.ElapsedMilliseconds;
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Surname { get; set; }
        }
    }

    public class QueryObject
    {
        public List<QueryCondition> QueryConditions { get; set; }
    }

    public class QueryCondition
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public string Operation { get; set; }
    }

    public static class QueryExtentions
    {
        public static Expression<Func<T, bool>> BuildExpression<T>(this QueryObject condition)
        {
            Expression<Func<T, bool>> expression = p => true;
            foreach (var queryCondition in condition.QueryConditions)
            {
                expression = expression.And(queryCondition.BuildExpression<T>());
            }
            return expression;
        }

        public static Expression<Func<T, bool>> BuildExpression<T>(this QueryCondition condition)
        {
            switch (condition.Operation)
            {
                case "Equal":
                case "GreaterThan":
                case "GreaterThanOrEqual":
                case "LessThan":
                case "LessThanOrEqual":
                case "NotEqual":
                    return BuildBinaryExpression<T>(condition);
                case "Contains":
                    return BuildMethodCallExpression<T>(condition);
                default:
                    throw new NotImplementedException(condition.Operation + "Not implemented!!");
            }
        }

        private static Expression<Func<T, bool>> BuildMethodCallExpression<T>(QueryCondition condition)
        {
            if (condition.Operation == "Contains")
            {
                var parameterExp = Expression.Parameter(typeof (T), "p");
                var property = Expression.Property(parameterExp, condition.PropertyName);
                var contains = typeof (string).GetMethod("Contains", new[] {typeof (string)});
                var someValue = Expression.Constant(condition.Value, typeof (string));
                var containsMethodExp = Expression.Call(property, contains, someValue);
                return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
            }
            throw new NotImplementedException();
        }

        private static Expression<Func<T, bool>> BuildBinaryExpression<T>(QueryCondition condition)
        {
            var type = typeof(T);
            var prop = GetProperty<T>(condition.PropertyName);
            var parameterExpression = Expression.Parameter(type, "p");
            Expression left = Expression.Property(parameterExpression, prop);
            Expression right = Expression.Convert(ToExprConstant(prop, condition.Value), prop.PropertyType);
            BinaryExpression filterOperation;
            switch (condition.Operation)
            {
                case "Equal":
                    filterOperation = Expression.Equal(left, right);
                    break;
                case "GreaterThan":
                    filterOperation = Expression.GreaterThan(left, right);
                    break;
                case "GreaterThanOrEqual":
                    filterOperation = Expression.GreaterThanOrEqual(left, right);
                    break;
                case "LessThan":
                    filterOperation = Expression.LessThan(left, right);
                    break;
                case "LessThanOrEqual":
                    filterOperation = Expression.LessThanOrEqual(left, right);
                    break;
                case "NotEqual":
                    filterOperation = Expression.NotEqual(left, right);
                    break;
                default:
                    throw new NotImplementedException(condition.Operation + "Not implemented");
            }
            return Expression.Lambda<Func<T, bool>>(filterOperation, parameterExpression);
        }

        private static Expression ToExprConstant(PropertyInfo prop, object value)
        {
            object val;
            switch (prop.PropertyType.Name)
            {
                case "System.Guid":
                    val = new Guid(value.ToString());
                    break;
                default:
                    val = Convert.ChangeType(value, prop.PropertyType);
                    break;
            }
            return Expression.Constant(val);
        }

        private static PropertyInfo GetProperty<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName);
        }
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
    }
}