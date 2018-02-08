namespace DAL.Fundamentals.Specifications
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using DAL.Fundamentals.Data;

    public class FilterSpecification<T> : Specification<T>
    {
        private List<FilterCondition> conditions = new List<FilterCondition>();

        public FilterSpecification(List<FilterCondition> conditions)
        {
            this.conditions = conditions;
        }

        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression left = Expression.Parameter(typeof(T), "c");
            Expression otherColumn = Expression.Constant(true);
            foreach (var filter in conditions)
            {
                Expression sameColumn = Expression.Constant(false);
                foreach (var propertyValue in filter.PropertyValues)
                {
                    Expression right = left;
                    filter.PropertyName.Split('.').ToList().ForEach(item => right = Expression.Property(right, item));
                    var finalValue = ConvertToRealValue(propertyValue, right);
                    var body = SetOperate(filter.FilterOperate, right, Expression.Constant(finalValue, right.Type));
                    sameColumn = Expression.Or(body, sameColumn);
                }
                otherColumn = Expression.And(sameColumn, otherColumn);
            }
            Expression<Func<T, bool>> finalExpression
                = Expression.Lambda<Func<T, bool>>(otherColumn, new ParameterExpression[] { left });
            return finalExpression;
        }

        private object ConvertToRealValue(object src, Expression dest)
        {
            object result = src;
            if (src != null)
            {
                Type srcType = src.GetType();
                if (srcType != dest.Type)
                {
                    if (dest.Type == typeof(DateTime) && srcType == typeof(long))
                    {
                        result = new DateTime((Int64)src, DateTimeKind.Utc);
                    }
                    else
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(dest.Type);
                        result = tc.ConvertFromString(src.ToString());
                    }
                }
            }
            return result;
        }

        private Expression SetOperate(FilterOperate filterOperate, Expression expression, ConstantExpression constantExpression)
        {
            Expression result = null;
            switch (filterOperate)
            {
                case FilterOperate.Equals:
                    result = Expression.Equal(expression, constantExpression);
                    break;

                case FilterOperate.NotEquals:
                    result = Expression.NotEqual(expression, constantExpression);
                    break;

                case FilterOperate.GreaterThan:
                    result = Expression.GreaterThan(expression, constantExpression);
                    break;

                case FilterOperate.GreaterThanOrEquals:
                    result = Expression.GreaterThanOrEqual(expression, constantExpression);
                    break;

                case FilterOperate.LessThan:
                    result = Expression.LessThan(expression, constantExpression);
                    break;

                case FilterOperate.LessThanOrEquals:
                    result = Expression.LessThanOrEqual(expression, constantExpression);
                    break;

                case FilterOperate.Contains:
                    result = Expression.Call(expression,
                        typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
                        constantExpression);
                    break;

                default:
                    break;
            }
            return result;
        }
    }
}