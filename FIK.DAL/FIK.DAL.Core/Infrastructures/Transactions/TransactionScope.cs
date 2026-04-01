using System;
using System.Data;
using System.Threading.Tasks;

namespace FIK.ORM.Infrastructures.Transactions
{
    /// <summary>
    /// Default implementation of ITransactionScope
    /// Handles transaction lifecycle with proper resource management
    /// </summary>
    public class TransactionScope : ITransactionScope
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _isCompleted;
        private bool _isDisposed;

        public IDbTransaction Transaction => _transaction;
        public IsolationLevel IsolationLevel { get; }
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// Creates a new transaction scope
        /// </summary>
        /// <param name="connection">The database connection to use</param>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <exception cref="ArgumentNullException">Thrown when connection is null</exception>
        public TransactionScope(IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _connection = connection;
            IsolationLevel = isolationLevel;

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        /// <inheritdoc/>
        public void Commit()
        {
            ThrowIfDisposed();
            ThrowIfCompleted();

            try
            {
                _transaction?.Commit();
                _isCompleted = true;
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Rollback()
        {
            ThrowIfDisposed();

            if (!_isCompleted && _transaction != null)
            {
                _transaction.Rollback();
                _isCompleted = true;
            }
        }



        //todo: Future enhancement - support savepoints if the underlying provider supports it
        //public void CreateSavepoint(string savepointName)
        //{
        //    ThrowIfDisposed();
        //    ThrowIfCompleted();

        //    if (string.IsNullOrWhiteSpace(savepointName))
        //        throw new ArgumentException("Savepoint name cannot be null or empty", nameof(savepointName));

        //    try
        //    {
        //        _transaction?.Save(savepointName);
        //    }
        //    catch (NotSupportedException ex)
        //    {
        //        throw new NotSupportedException($"Database provider does not support savepoints", ex);
        //    }
        //}

        //todo: Future enhancement - support savepoints if the underlying provider supports it
        //public void RollbackToSavepoint(string savepointName)
        //{
        //    ThrowIfDisposed();
        //    ThrowIfCompleted();

        //    if (string.IsNullOrWhiteSpace(savepointName))
        //        throw new ArgumentException("Savepoint name cannot be null or empty", nameof(savepointName));

        //    try
        //    {

        //       _transaction?.Rollback(savepointName);
        //    }
        //    catch (NotSupportedException ex)
        //    {
        //        throw new NotSupportedException($"Database provider does not support savepoints", ex);
        //    }
        //}

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                // Auto-rollback if not completed
                if (!_isCompleted)
                    Rollback();
            }
            finally
            {
                _transaction?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _isDisposed = true;
            }
        }

       

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void ThrowIfCompleted()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Transaction has already been completed");
        }



#if NET6_0_OR_GREATER
        public async Task CommitAsync()
        {
            ThrowIfDisposed();
            ThrowIfCompleted();

            try
            {
                // For non-async ADO.NET providers, wrap in Task
                //await Task.FromResult(_transaction?.Commit());
                //await Task.Run(() => _transaction?.Commit());

                _transaction?.Commit();
                _isCompleted = true;
                await Task.CompletedTask;
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            ThrowIfDisposed();

            if (!_isCompleted && _transaction != null)
            {
                //await Task.FromResult(_transaction.Rollback());
                _transaction.Rollback();
                _isCompleted = true;
                await Task.CompletedTask;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            try
            {
                if (!_isCompleted)
                    await RollbackAsync();
            }
            finally
            {
                _transaction?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _isDisposed = true;
            }
        }
#endif



    }
}