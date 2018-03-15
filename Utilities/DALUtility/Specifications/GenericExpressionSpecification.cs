namespace DALUtility.Specifications
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class GenericExpressionSpecification<TEntity, TResult>
    {
        private readonly string propertyName;

        public GenericExpressionSpecification(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public Expression<Func<TEntity, TResult>> GetExpression()
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            ParameterExpression left = Expression.Parameter(typeof(TEntity), "c");
            Expression right = left;
            propertyName.Split('.').ToList().ForEach(item => right = Expression.Property(right, item));
            Expression<Func<TEntity, TResult>> finalExpression
                = Expression.Lambda<Func<TEntity, TResult>>(right, new ParameterExpression[] { left });
            return finalExpression;
        }
    }
}