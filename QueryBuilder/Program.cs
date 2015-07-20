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
            var qc = new QueryCondition<Person>
            {
                PropertyName = "Name",
                Operation = "Contains",
                Value = "ana"
            };

            var list = new List<Person>
            {
                new Person {Name = "B"},
                new Person {Name = "A"},
                new Person {Name = "Ana-Maria"}
            };
            var ex = qc.BuildExpression().Compile();
            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < 100000; i++)
            {
                var s = list.Where(p => p.Name.Contains("ana")).ToList();
                //var s = list.Where(ex).ToList();
            }

            watch.Stop();
            var ms = watch.ElapsedMilliseconds;
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }


    public class QueryCondition<T>
    {
        public T TargetType { get; set; }
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public string Operation { get; set; }
    }

    public static class QueryExtentions
    {
        public static Expression<Func<T, bool>> BuildExpression<T>(this QueryCondition<T> condition)
        {
            switch (condition.Operation)
            {
                case "Equal":
                case "GreaterThan":
                case "GreaterThanOrEqual":
                case "LessThan":
                case "LessThanOrEqual":
                case "NotEqual":
                    return BuildBinaryExpression(condition);
                case "Contains":
                    return BuildMethodCallExpression(condition);
                default:
                    throw new NotImplementedException(condition.Operation + "Not implemented!!");
            }
        }

        private static Expression<Func<T, bool>> BuildMethodCallExpression<T>(QueryCondition<T> condition)
        {
            var parameterExp = Expression.Parameter(typeof(T), "p");
            var property = Expression.Property(parameterExp, condition.PropertyName);
            var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(condition.Value, typeof(string));
            var containsMethodExp = Expression.Call(property, contains, someValue);
            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }

        private static Expression<Func<T, bool>> BuildBinaryExpression<T>(QueryCondition<T> condition)
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