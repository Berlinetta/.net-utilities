namespace DAL.Fundamentals.Specifications
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class DynamicExpressionSpecification<T>
    {
        private readonly string propertyName;

        public DynamicExpressionSpecification(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public Expression<Func<T, dynamic>> GetExpression()
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            ParameterExpression left = Expression.Parameter(typeof(T), "c");
            Expression right = left;
            propertyName.Split('.').ToList().ForEach(item => right = Expression.Property(right, item));
            Expression<Func<T, dynamic>> finalExpression
                = Expression.Lambda<Func<T, dynamic>>(Expression.Convert(right, typeof(object)), new ParameterExpression[] { left });
            return finalExpression;
        }
    }
}