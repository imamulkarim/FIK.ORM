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

        //Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Executes a callback function within a transaction scope
        /// Automatically commits on success, rolls back on exception
        /// </summary>
        /// <typeparam name="TResult">The return type of the operation</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <returns>The result of the operation</returns>
        TResult ExecuteInTransaction<TResult>(Func<ITransactionScope, TResult> operation);

        //Task<TResult> ExecuteInTransactionAsync<TResult>(Func<ITransactionScope, Task<TResult>> operation);

        /// <summary>
        /// Executes a callback function within a transaction scope (no return value)
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        void ExecuteInTransaction(Action<ITransactionScope> operation);

        //Task ExecuteInTransactionAsync(Func<ITransactionScope, Task> operation);

        /// <summary>
        /// Executes the specified command within a transaction and maps the result set to objects.
        /// </summary>
        /// <typeparam name="T">The target model type.</typeparam>
        /// <param name="dbCommand">The command to execute.</param>
        /// <param name="metaDatas">The metadata describing the columns to map.</param>
        /// <returns>An enumerable sequence of mapped objects.</returns>
        IEnumerable<T> ExecuteInTransaction<T>(IDbCommand dbCommand, MetaDataInfo[] metaDatas) where T : class, new();

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        void Commit();

    }
}
