using CharCode.Base.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CharCode.Base.Extentions
{
    public static class IQueryableExtention
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering)
        {
            if (string.IsNullOrWhiteSpace(ordering))
                return source;

            var type = typeof(T);
            var property = GetPropertyType<T>(ordering);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = ExpressionRetriever.GetMember(parameter, ordering);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var resultExp = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string ordering)
        {
            if (string.IsNullOrWhiteSpace(ordering))
                return source;

            var type = typeof(T);
            var property = GetPropertyType<T>(ordering);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = ExpressionRetriever.GetMember(parameter, ordering);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var resultExp = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }

        private static PropertyInfo GetPropertyType<T>(string propertyName)
        {
            var type = typeof(T);
            PropertyInfo result = null;
            foreach (var property in propertyName.Split('.'))
            {
                result = type.GetProperties().Single(p => p.Name.ToLower().Equals(property.ToLower()));
                type = result.PropertyType;
            }

            return result;
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> source, ExpressionFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.PropertyName))
                return source;

            var type = typeof(T);
            var parameter = Expression.Parameter(type, "p");
            var expression = ExpressionRetriever.GetExpression<T>(parameter, filter);

            var lambdaExpression = Expression.Lambda<Func<T, bool>>(expression, parameter);
            return source.Where(lambdaExpression);
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> source, PaginationConfig config)
        {
           if (config.Logic == ExpressionFilterLogic.And)
                return FilterByAndLogic(source, config);
            else
                return FilterByOrLogic(source, config);
        }

        private static IQueryable<T> FilterByOrLogic<T>(IQueryable<T> source, PaginationConfig config)
        {
            IQueryable<T> result = null;

            foreach (var filter in config.ExpressionFilters)
            {
                if (result == null)
                    result = source.Filter(filter);
                else
                    result = result.Union(source.Filter(filter));
            }

            return result;
        }

        private static IQueryable<T> FilterByAndLogic<T>(IQueryable<T> source, PaginationConfig config)
        {
            var result = source;
            foreach (var filter in config.ExpressionFilters)
                result = result.Filter(filter);

            return result;
        }
    }
}
