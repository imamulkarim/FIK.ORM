using System;
using System.Data;
using System.Threading.Tasks;

namespace FIK.ORM.Infrastructures.Transactions
{
    /// <summary>
    /// Represents a database transaction scope
    /// Handles automatic commit/rollback and resource management
    /// </summary>
    public interface ITransactionScope : IDisposable
        #if NET6_0_OR_GREATER
            , IAsyncDisposable
        #endif

    {
        /// <summary>
        /// Gets the underlying database transaction
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Gets the isolation level for this transaction
        /// </summary>
        IsolationLevel IsolationLevel { get; }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        void Commit();

#if NET6_0_OR_GREATER
        /// <summary>
        /// Commits the transaction asynchronously
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rolls back the transaction asynchronously
        /// </summary>
        Task RollbackAsync();
#endif
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        void Rollback();

        /// <summary>
        /// Creates a savepoint within the transaction
        /// </summary>
        //void CreateSavepoint(string savepointName);

        /// <summary>
        /// Rolls back to a specific savepoint
        /// </summary>
        //void RollbackToSavepoint(string savepointName);

        /// <summary>
        /// Indicates whether the transaction has been committed or rolled back
        /// </summary>
        bool IsCompleted { get; }
    }
}