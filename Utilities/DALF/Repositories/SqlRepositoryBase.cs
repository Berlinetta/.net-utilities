using System.Collections.Generic;
using System.Data;

namespace DAL.Fundamentals.Repositories
{
    public abstract class SqlRepositoryBase : ISql
    {
        protected IRepositoryContext efContext;

        public SqlRepositoryBase()
        {
        }

        public SqlRepositoryBase(IRepositoryContext context)
        {
            this.efContext = context;
        }

        #region ISql Methods

        public int ExecuteCommand(string sqlCommand, params object[] parameters)
        {
            return efContext.Context.Database.ExecuteSqlCommand(sqlCommand, parameters);
        }

        public IEnumerable<TEntity> ExecuteQuery<TEntity>(string sqlQuery, params object[] parameters)
        {
            return efContext.Context.Database.SqlQuery<TEntity>(sqlQuery, parameters);
        }

        public int ExecuteNonQueryWithTransaction(IEnumerable<ExecutionUnit> executionUnits)
        {
            int result;
            var conn = this.efContext.Context.Database.Connection;
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        foreach (var unit in executionUnits)
                        {
                            command.CommandText = unit.Name;
                            command.Parameters.Clear();
                            command.Parameters.AddRange(unit.Parameters);
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    result = 1;
                }
                catch
                {
                    //Logger.Instance.Error("An error occurred when Execute NonQuery With Transaction.");
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        #endregion ISql Methods
    }
}
