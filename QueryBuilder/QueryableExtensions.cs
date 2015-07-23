using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "OrderByDescending");
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "ThenBy");
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "ThenByDescending");
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        public static IQueryable<T> GetSortedPage<T>(this IQueryable<T> source, GridCriteria gridCriteria)
        {
            if (source == null || gridCriteria == null)
            {
                return source;
            }
            return source.OrderBy(gridCriteria.SortCriteria)
                .Skip(gridCriteria.Pagination.StartIndex)
                .Take(gridCriteria.Pagination.PageSize);
        }

        public static IList<T> GetSortedPageAsList<T>(this IQueryable<T> source, GridCriteria gridCriteria)
        {
            return source == null ? null : GetSortedPage(source, gridCriteria).ToList();
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, List<SortCriteria> sortData)
        {
            if (sortData == null || sortData.Count == 0)
            {
                return source;
            }
            IOrderedQueryable<T> result = null;
            for (var i = 0; i < sortData.Count; i++)
            {
                var sort = sortData[i];
                var property = sort.PropertyName;
                switch (sort.SortOrder)
                {
                    case SortOrder.Ascendant:
                        result = i == 0 ? source.OrderBy(property) : result.ThenBy(property);
                        break;
                    case SortOrder.Descendant:
                        result = i == 0 ? source.OrderByDescending(property) : result.ThenByDescending(property);
                        break;
                }
            }
            return result;
        }
    }
}