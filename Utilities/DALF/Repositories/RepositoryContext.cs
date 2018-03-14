namespace DAL.Fundamentals.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Validation;

    /// <summary>
    /// Represents the base class for repository contexts.
    /// </summary>
    public class RepositoryContext<TDbContext> : IRepositoryContext where TDbContext : DbContext
    {
        private TDbContext localCtx;

        private bool localCommitted = true;

        #region IRepositoryContext

        public RepositoryContext(TDbContext dbContext)
        {
            localCtx = dbContext;
        }

        public TDbContext CurrentContext
        {
            get { return localCtx; }
        }

        public void RegisterDeleted<TEntity>(TEntity obj) where TEntity : class
        {
            CurrentContext.Set<TEntity>().Remove(obj);
            Committed = false;
        }

        public void RegisterModified<TEntity>(TEntity oEntity, TEntity nEntity) where TEntity : class
        {
            CurrentContext.Entry(oEntity).CurrentValues.SetValues(nEntity);
            Committed = false;
        }

        public void RegisterNew<TEntity>(TEntity obj) where TEntity : class
        {
            CurrentContext.Set<TEntity>().Add(obj);
            Committed = false;
        }

        #endregion IRepositoryContext

        #region IUnitOfWork

        public bool Committed
        {
            get { return localCommitted; }
            protected set { localCommitted = value; }
        }

        public void Commit()
        {
            if (!Committed)
            {
                Committed = true;
                var validationErrors = new List<DbEntityValidationResult>(CurrentContext.GetValidationErrors());
                foreach (var validationError in validationErrors)
                {
                    var entity = validationError.Entry.Entity;
                    CurrentContext.Entry(entity).State = EntityState.Detached;
                }

                CurrentContext.SaveChanges();
                if (validationErrors.Count > 0)
                {
                    throw new DbEntityValidationException("There're validation errors, commit failed.", validationErrors);
                }
            }
        }

        public void Rollback()
        {
            Committed = false;
        }

        #endregion IUnitOfWork

        #region IDisposable

        public void Dispose()
        {
            if (!Committed)
            {
                Commit();
            }

            CurrentContext.Dispose();
        }

        #endregion IDisposable

        DbContext IRepositoryContext.Context
        {
            get { return localCtx; }
        }
    }
}