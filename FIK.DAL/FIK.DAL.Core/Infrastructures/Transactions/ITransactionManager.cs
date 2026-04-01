using FIK.ORM.Infrastructures.MetaData;
using System;
using System.Collections.Generic;
using System.Data;

namespace FIK.ORM.Infrastructures.Transactions
{
    /// <summary>
    /// Manages database transactions for a data access provider
    /// </summary>
    public interface ITransactionManager : IDisposable
        #if NET6_0_OR_GREATER
            , IAsyncDisposable
#endif
    {
        /// <summary>
        /// Gets the current active transaction scope, or null if no transaction is active
        /// </summary>
        ITransactionScope CurrentTransaction { get; }

        /// <summary>
        /// Creates a new transaction scope
        /// </summary>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <returns>A new transaction scope</returns>
        ITransactionScope BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Creates a new transaction scope asynchronously
        /// </summary>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <returns>A task that returns a new transaction scope</returns>
        //Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Executes a callback function within a transaction scope
        /// Automatically commits on success, rolls back on exception
        /// </summary>
        /// <typeparam name="TResult">The return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <returns>The result of the operation</returns>
        TResult ExecuteInTransaction<TResult>(Func<ITransactionScope, TResult> operation);

        /// <summary>
        /// Executes a callback function within a transaction scope asynchronously
        /// </summary>
        /// <typeparam name="TResult">The return type of the operation</typeparam>
        /// <param name="operation">The async operation to execute</param>
        /// <returns>A task that returns the result of the operation</returns>
        //Task<TResult> ExecuteInTransactionAsync<TResult>(Func<ITransactionScope, Task<TResult>> operation);

        /// <summary>
        /// Executes a callback function within a transaction scope (no return value)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        void ExecuteInTransaction(Action<ITransactionScope> operation);

        /// <summary>
        /// Executes a callback function within a transaction scope asynchronously (no return value)
        /// </summary>
        /// <param name="operation">The async operation to execute</param>
        //Task ExecuteInTransactionAsync(Func<ITransactionScope, Task> operation);

        IEnumerable<T> ExecuteInTransaction<T>(IDbConnection iDbConnection, IDbCommand oCmd, MetaDataInfo[] metaDatas) where T : class, new();
    }
}