﻿namespace DAL.Fundamentals.Repositories
{
    using DAL.Fundamentals.Data;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// 表示基于Entity Framework的排序扩展类型。该扩展解决了在Entity Framework上针对某些
    /// 原始数据类型执行排序操作时，出现Expression of type A cannot be used for return type B
    /// 错误的问题。
    /// </summary>
    /// <remarks>有关该功能扩展的更多信息，请参考：http://www.cnblogs.com/daxnet/archive/2012/07/23/2605695.html。</remarks>
    public static class SortByExtension
    {
        #region public Methods

        public static IOrderedQueryable<TEntity> Sort<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder order)
            where TEntity : class
        {
            return InvokeSortBy(query, sortPredicate, order);
        }

        public static IOrderedQueryable<TEntity> SortBy<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, dynamic>> sortPredicate)
            where TEntity : class
        {
            return InvokeSortBy(query, sortPredicate, SortOrder.ASC);
        }

        public static IOrderedQueryable<TEntity> SortByDescending<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, dynamic>> sortPredicate)
            where TEntity : class
        {
            return InvokeSortBy(query, sortPredicate, SortOrder.DESC);
        }

        #endregion public Methods

        #region Private Methods

        private static IOrderedQueryable<TEntity> InvokeSortBy<TEntity>(IQueryable<TEntity> query,
            Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder)
            where TEntity : class
        {
            var param = sortPredicate.Parameters[0];
            string propertyName = null;
            Type propertyType = null;
            Expression bodyExpression = null;
            if (sortPredicate.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = sortPredicate.Body as UnaryExpression;
                bodyExpression = unaryExpression.Operand;
            }
            else if (sortPredicate.Body is MemberExpression)
            {
                bodyExpression = sortPredicate.Body;
            }
            else
                throw new ArgumentException(@"The body of the sort predicate expression should be
                either UnaryExpression or MemberExpression.", "sortPredicate");
            MemberExpression memberExpression = (MemberExpression)bodyExpression;
            propertyName = memberExpression.Member.Name;
            if (memberExpression.Member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
                propertyType = propertyInfo.PropertyType;
            }
            else
                throw new InvalidOperationException(@"Cannot evaluate the type of property since the member expression
                represented by the sort predicate expression does not contain a PropertyInfo object.");

            Type funcType = typeof(Func<,>).MakeGenericType(typeof(TEntity), propertyType);
            LambdaExpression convertedExpression = Expression.Lambda(funcType,
                Expression.Convert(Expression.Property(param, propertyName), propertyType), param);

            var sortingMethods = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var sortingMethodName = GetSortingMethodName(sortOrder);
            var sortingMethod = sortingMethods.Where(sm => sm.Name == sortingMethodName &&
                sm.GetParameters() != null &&
                sm.GetParameters().Length == 2).First();
            return (IOrderedQueryable<TEntity>)sortingMethod
                .MakeGenericMethod(typeof(TEntity), propertyType)
                .Invoke(null, new object[] { query, convertedExpression });
        }

        private static string GetSortingMethodName(SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case SortOrder.ASC:
                    return "OrderBy";

                case SortOrder.DESC:
                    return "OrderByDescending";

                default:
                    throw new ArgumentException("Sort Order must be specified as either Ascending or Descending.",
            "sortOrder");
            }
        }

        #endregion Private Methods
    }
}