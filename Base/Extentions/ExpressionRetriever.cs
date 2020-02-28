using CharCode.Base.Classes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CharCode.Base.Extentions
{
    public static class ExpressionRetriever
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        public static Expression GetExpression<T>(ParameterExpression param, ExpressionFilter filter)
        {
            var member = GetMember(param, filter.PropertyName);
            Expression constant = Expression.Constant(filter.Value);

            if (filter.Value is string value)
            {
                filter.Value = value.ToLower();

                member = Expression.Call(member, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                constant = Expression.Call(constant, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            }

            switch (filter.Comparison)
            {
                case Comparison.Equal:
                    return Expression.Equal(member, constant);
                case Comparison.GreaterThan:
                    return Expression.GreaterThan(member, constant);
                case Comparison.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);
                case Comparison.LessThan:
                    return Expression.LessThan(member, constant);
                case Comparison.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);
                case Comparison.NotEqual:
                    return Expression.NotEqual(member, constant);
                case Comparison.Contains:
                    return Expression.Call(member, containsMethod, constant);
                case Comparison.StartsWith:
                    return Expression.Call(member, startsWithMethod, constant);
                case Comparison.EndsWith:
                    return Expression.Call(member, endsWithMethod, constant);
                default:
                    throw new NotSupportedException();
            }
        }

        public static Expression GetMember(ParameterExpression param, string propertyName)
        {
            Expression member = param;

            foreach(var item in propertyName.Split('.'))
                member = Expression.Property(member, item);

            return member;
        }
    }
}
