namespace DAL.Fundamentals.Repositories
{
    #region using directives

    using System;
    using System.Data.Common;

    #endregion using directives

    /// <summary>
    /// Sql存储过程执行单元
    /// </summary>
    public class ExecutionUnit
    {
        /// <summary>
        /// 存储过程名称
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 存储过程参数
        /// </summary>
        public DbParameter[] Parameters { get; set; }
    }
}