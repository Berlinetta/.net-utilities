namespace DAL.Fundamentals.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DAL.Fundamentals.Data;
    using DAL.Fundamentals.Specifications;

    /// <summary>
    /// Base interface for implement a "Repository Pattern", for
    /// more information about this pattern see http://martinfowler.com/eaaCatalog/repository.html
    /// or http://blogs.msdn.com/adonet/archive/2009/06/16/using-repository-and-unit-of-work-patterns-with-entity-framework-4-0.aspx
    /// </summary>
    /// <remarks>
    /// Indeed, one might think that IDbSet already a generic repository and therefore
    /// would not need this item. Using this interface allows us to ensure PI principle
    /// within our domain model
    /// </remarks>
    /// <typeparam name="TEntity">Type of entity for this repository </typeparam>
    public interface IRepository<TEntity>
        where TEntity : class
    {
        #region Properties

        /// <summary>
        ///  Gets the instance of the repository context on which the repository was attached.
        /// </summary>
        IRepositoryContext Context { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Add item into repository
        /// </summary>
        /// <param name="item">Item to add to repository</param>

        void Add(TEntity item);

        void Add(List<TEntity> items);

        #region Get Series

        /// <summary>
        /// Get element by entity key
        /// </summary>
        /// <param name="id">Entity key value</param>
        /// <returns></returns>
        TEntity Get(ISpecification<TEntity> specification, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        TEntity Get(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        #endregion Get Series

        #region GetAll Series

        /// <summary>
        /// Get all elements of type TEntity in repository
        /// </summary>
        /// <returns>List of selected elements</returns>
        IList<T> GetColumnValues<T>(ISpecification<TEntity> specification, Expression<Func<TEntity, T>> distinctProperty);

        IList<TEntity> GetAll(params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        IList<TEntity> GetAll(Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        IList<TEntity> GetAll(ISpecification<TEntity> specification, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        IList<TEntity> GetAll(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        #endregion GetAll Series

        #region GetPaged Series

        PagedResult<TEntity> GetPaged(int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        PagedResult<TEntity> GetPaged(Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        PagedResult<TEntity> GetPaged(ISpecification<TEntity> specification, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        PagedResult<TEntity> GetPaged(ISpecification<TEntity> specification, Expression<Func<TEntity, dynamic>> sortPredicate, SortOrder sortOrder, int pageNumber, int pageSize, params Expression<Func<TEntity, dynamic>>[] eagerLoadingProperties);

        #endregion GetPaged Series

        /// <summary>
        /// Checkes whether the aggregate root, which matches the given specification, exists in the repository.
        /// </summary>
        /// <param name="specification">The specification with which the aggregate root should match.</param>
        /// <returns>True if the aggregate root exists, otherwise false.</returns>
        bool Exists(ISpecification<TEntity> specification);

        /// <summary>
        /// Removes the aggregate root from current repository.
        /// </summary>
        /// <param name="item">The aggregate root to be removed.</param>
        void Remove(TEntity item);

        void RemoveAll(ISpecification<TEntity> specification);

        /// <summary>
        /// Updates the aggregate root in the current repository.
        /// </summary>
        /// <param name="item">The aggregate root to be updated.</param>
        void Update(ISpecification<TEntity> specification, TEntity item);

        void SaveOrUpdate(ISpecification<TEntity> specification, TEntity item);

        void SaveOrUpdate(IDictionary<ISpecification<TEntity>, TEntity> items);

        #endregion Methods
    }
}