namespace DAL.Fundamentals.Repositories
{
    using System.Collections.Generic;

    /// <summary>
    /// 定义Sql常用操作的接口
    /// </summary>
    public interface ISql
    {
        /// <summary>
        /// Execute specific query with underlying persistence store
        /// </summary>
        /// <typeparam name="TEntity">Entity type to map query results</typeparam>
        /// <param name="sqlQuery">
        /// Dialect Query
        /// <example>
        /// SELECT idCustomer,Name FROM dbo.[Customers] WHERE idCustomer > {0}
        /// </example>
        /// </param>
        /// <param name="parameters">A vector of parameters values</param>
        /// <returns>
        /// Enumerable results
        /// </returns>
        IEnumerable<TEntity> ExecuteQuery<TEntity>(string sqlQuery, params object[] parameters);

        /// <summary>
        /// Execute arbitrary command into underlying persistence store
        /// </summary>
        /// <param name="sqlCommand">
        /// Command to execute
        /// <example>
        /// SELECT idCustomer,Name FROM dbo.[Customers] WHERE idCustomer > {0}
        /// </example>
        ///</param>
        /// <param name="parameters">A vector of parameters values</param>
        /// <returns>The number of affected records</returns>
        int ExecuteCommand(string sqlCommand, params object[] parameters);

        /// <summary>
        /// Execute multiple stored procedure in the same transaction unit.
        /// </summary>
        /// <param name="funcUnits"></param>
        /// <returns></returns>
        int ExecuteNonQueryWithTransaction(IEnumerable<ExecutionUnit> executionUnitsUnits);
    }
}