using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using DALUtility.Data;
using DALUtility.Specifications;

namespace DALUtility.Repositories
{
    public abstract class RepositoryBase<TEntity> : SqlRepositoryBase, IRepository<TEntity>
        where TEntity : class
    {
        private readonly IRepositoryContext efContext;

        public RepositoryBase(IRepositoryContext context)
            : base(context)
        {
            if (context == (IRepositoryContext)null)
                throw new ArgumentNullException("context");
            this.efContext = context;
            ((IObjectContextAdapter)this.efContext.Context).ObjectContext.CommandTimeout = FundamentalsConfiguration.CommandTimeout;
        }

        #region IRepository Methods

        public IRepositoryContext Context
        {
            get { return this.efContext; }
        }

        public void Add(TEntity item)
        {
            efContext.RegisterNew<TEntity>(item);
            efContext.Commit();
        }

        public void Add(List<TEntity> items)
        {
            items.ForEach(item => efContext.RegisterNew<TEntity>(item));
            efContext.Commit();
        }

        public bool Exists(ISpecification<TEntity> specification)
        {
            var count = efContext.Context.Set<TEntity>().Count(specification.GetExpression());
            return count != 0;
        }

        public int Count(ISpecification<TEntity> specification)
        {
            return efContext.Context.Set<TEntity>().Count(specification.GetExpression());
        }

        public TEntity Get(ISpecification<TEntity> specification, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFind(specification, null, SortOrder.Unspecified, eagerLoadingProperties);
        }

        public TEntity Get(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFind(specification, sortPredicate, sortOrder, eagerLoadingProperties);
        }

        public IList<T> GetColumnValues<T>(ISpecification<TEntity> specification, Expression<Func<TEntity, T>> distinctProperty)
        {
            var result = new List<T>();
            var dbset = efContext.Context.Set<TEntity>();
            var queryable = dbset.Where(specification.GetExpression()).Select(distinctProperty).Distinct();
            queryable.ToList().ForEach(item => result.Add((T)item));
            return result;
        }

        public IList<TEntity> GetAll(params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(new AnySpecification<TEntity>(), null, SortOrder.Unspecified, eagerLoadingProperties);
        }

        public IList<TEntity> GetAll(Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(new AnySpecification<TEntity>(), sortPredicate, sortOrder, eagerLoadingProperties);
        }

        public IList<TEntity> GetAll(ISpecification<TEntity> specification, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(specification, null, SortOrder.Unspecified, eagerLoadingProperties);
        }

        public IList<TEntity> GetAll(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params System.Linq.Expressions.Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(specification, sortPredicate, sortOrder, eagerLoadingProperties);
        }

        public PagedResult<TEntity> GetPaged(int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(new AnySpecification<TEntity>(), null, SortOrder.Unspecified, pageNumber, pageSize, eagerLoadingProperties);
        }

        public PagedResult<TEntity> GetPaged(Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(new AnySpecification<TEntity>(), sortPredicate, sortOrder, pageNumber, pageSize, eagerLoadingProperties);
        }

        public PagedResult<TEntity> GetPaged(ISpecification<TEntity> specification, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(specification, null, SortOrder.Unspecified, pageNumber, pageSize, eagerLoadingProperties);
        }

        public PagedResult<TEntity> GetPaged(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            return this.DoFindAll(specification, sortPredicate, sortOrder, pageNumber, pageSize, eagerLoadingProperties);
        }

        public void Remove(TEntity item)
        {
            efContext.RegisterDeleted<TEntity>(item);
            efContext.Commit();
        }

        public void RemoveAll(ISpecification<TEntity> specification)
        {
            var entitise = GetAll(specification);
            if (entitise != null)
            {
                var entityCounts = entitise.Count;
                while (true)
                {
                    if (entityCounts >= 1000)
                    {
                        this.SetRegisterDeleted(entitise.ToList().Take(1000).ToList() as IList<TEntity>);
                        entityCounts = entityCounts - 1000;
                        entitise = entitise.Skip(1000).ToList().Take(entityCounts).ToList() as IList<TEntity>;
                    }
                    else
                    {
                        this.SetRegisterDeleted(entitise);
                        break;
                    }
                }
            }
        }

        public void SaveOrUpdate(ISpecification<TEntity> specification, TEntity item)
        {
            var entity = DoFind(specification, null, SortOrder.Unspecified);
            if (entity != null)
            {
                efContext.RegisterModified<TEntity>(entity, item);
            }
            else
            {
                efContext.RegisterNew<TEntity>(item);
            }
            efContext.Commit();
        }

        public void SaveOrUpdate(IDictionary<ISpecification<TEntity>, TEntity> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    var specification = item.Key;
                    var entity = DoFind(specification, null, SortOrder.Unspecified);
                    if (entity != null)
                    {
                        efContext.RegisterModified<TEntity>(entity, item.Value);
                    }
                    else
                    {
                        efContext.RegisterNew<TEntity>(item.Value);
                    }
                }
                efContext.Commit();
            }
        }

        public void Update(ISpecification<TEntity> specification, TEntity item)
        {
            var entity = DoFind(specification, null, SortOrder.Unspecified);
            if (entity != null)
            {
                efContext.RegisterModified<TEntity>(entity, item);
            }
            else
            {
                throw new ArgumentException("Can't find the record based on the specified query conditions.");
            }
            efContext.Commit();
        }

        #endregion IRepository Methods

        #region Private Methods

        private TEntity DoFind(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            var dbset = efContext.Context.Set<TEntity>();
            IQueryable<TEntity> queryable = null;
            if (eagerLoadingProperties != null &&
                eagerLoadingProperties.Length > 0)
            {
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbset.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                queryable = dbquery.Where(specification.GetExpression());
            }
            else
                queryable = dbset.Where(specification.GetExpression());
            if (sortPredicate != null)
            {
                switch (sortOrder)
                {
                    case SortOrder.ASC:
                        return queryable.SortBy(sortPredicate).FirstOrDefault();

                    case SortOrder.DESC:
                        return queryable.SortByDescending(sortPredicate).FirstOrDefault();

                    default:
                        break;
                }
            }
            return queryable.FirstOrDefault();
        }

        private IList<TEntity> DoFindAll(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            var dbset = efContext.Context.Set<TEntity>();
            IQueryable<TEntity> queryable = null;
            if (eagerLoadingProperties != null &&
                eagerLoadingProperties.Length > 0)
            {
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbset.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                queryable = dbquery.Where(specification.GetExpression());
            }
            else
                queryable = dbset.Where(specification.GetExpression());

            if (sortPredicate != null)
            {
                switch (sortOrder)
                {
                    case SortOrder.ASC:
                        return queryable.SortBy(sortPredicate).ToList();

                    case SortOrder.DESC:
                        return queryable.SortByDescending(sortPredicate).ToList();

                    default:
                        break;
                }
            }
            return queryable.ToList();
        }

        private PagedResult<TEntity> DoFindAll(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "The page number must be greater than or equal to 1.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "The page size must be greater than or equal to 1.");

            int skip = (pageNumber - 1) * pageSize;
            int take = pageSize;

            var dbset = efContext.Context.Set<TEntity>();
            IQueryable<TEntity> queryable = null;
            if (eagerLoadingProperties != null &&
                eagerLoadingProperties.Length > 0)
            {
                var eagerLoadingProperty = eagerLoadingProperties[0];
                var eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                var dbquery = dbset.Include(eagerLoadingPath);
                for (int i = 1; i < eagerLoadingProperties.Length; i++)
                {
                    eagerLoadingProperty = eagerLoadingProperties[i];
                    eagerLoadingPath = this.GetEagerLoadingPath(eagerLoadingProperty);
                    dbquery = dbquery.Include(eagerLoadingPath);
                }
                queryable = dbquery.Where(specification.GetExpression());
            }
            else
                queryable = dbset.Where(specification.GetExpression());

            int totalCount = queryable.Count();
            IQueryable<TEntity> queryResult = null;
            if (sortPredicate != null)
            {
                switch (sortOrder)
                {
                    case SortOrder.ASC:
                        queryResult = queryable.SortBy(sortPredicate).Skip(skip).Take(take);
                        break;

                    case SortOrder.DESC:
                        queryResult = queryable.SortByDescending(sortPredicate).Skip(skip).Take(take);
                        break;

                    default:
                        break;
                }
                if (queryResult == null)
                {
                    return new PagedResult<TEntity> { TotalPages = 1, TotalRecords = 0, PageSize = 1, PageNumber = 1 };
                }
                else
                {
                    return new PagedResult<TEntity>(totalCount, (totalCount + pageSize - 1) / pageSize, pageSize, pageNumber, queryResult.ToList());
                }
            }
            throw new InvalidOperationException("Must specify this sort field and a sort order.");
        }

        private string GetEagerLoadingPath(Expression<Func<TEntity, dynamic>> eagerLoadingProperty)
        {
            MemberExpression memberExpression = this.GetMemberInfo(eagerLoadingProperty);
            var parameterName = eagerLoadingProperty.Parameters.First().Name;
            var memberExpressionStr = memberExpression.ToString();
            var path = memberExpressionStr.Replace(parameterName + ".", "");
            return path;
        }

        private MemberExpression GetMemberInfo(LambdaExpression lambda)
        {
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }

        private void SetRegisterDeleted(IList<TEntity> entitise)
        {
            foreach (var item in entitise)
            {
                efContext.RegisterDeleted<TEntity>(item);
            }
            efContext.Commit();
        }

        #endregion Private Methods
    }
}