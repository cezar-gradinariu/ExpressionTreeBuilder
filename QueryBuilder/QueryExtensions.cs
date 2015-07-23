using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryBuilder
{
    public static class QueryExtensions
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

        public static Expression<Func<T, bool>> BuildExpression<T>(this QueryObject.QueryCondition condition)
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

        private static Expression<Func<T, bool>> BuildMethodCallExpression<T>(QueryObject.QueryCondition condition)
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

        private static Expression<Func<T, bool>> BuildBinaryExpression<T>(QueryObject.QueryCondition condition)
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
}