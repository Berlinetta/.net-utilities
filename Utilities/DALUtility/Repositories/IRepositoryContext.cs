namespace DALUtility.Repositories
{
    using System;
    using System.Data.Entity;

    /// <summary>
    /// Represents that the implemented classes are repository contexts.
    /// </summary>
    public interface IRepositoryContext : IUnitOfWork, IDisposable
    {
        /// <summary>
        /// Gets the internal DbContext of the repository context.
        /// </summary>
        DbContext Context { get; }

        /// <summary>
        /// Registers a new object to the repository context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the aggregate root.</typeparam>
        /// <param name="obj">The object to be registered.</param>
        void RegisterNew<TEntity>(TEntity obj)
            where TEntity : class;

        /// <summary>
        /// Registers a modified object to the repository context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the aggregate root.</typeparam>
        /// <param name="obj">The object to be registered.</param>
        void RegisterModified<TEntity>(TEntity oEntity, TEntity nEntity)
            where TEntity : class;

        /// <summary>
        /// Registers a deleted object to the repository context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the aggregate root.</typeparam>
        /// <param name="obj">The object to be registered.</param>
        void RegisterDeleted<TEntity>(TEntity obj)
            where TEntity : class;
    }
}