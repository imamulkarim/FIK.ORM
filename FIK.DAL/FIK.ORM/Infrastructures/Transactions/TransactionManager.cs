using FIK.ORM.Infrastructures.MetaData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace FIK.ORM.Infrastructures.Transactions
{
    /// <summary>
    /// Default implementation of ITransactionManager
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private readonly IDbConnection _connection;
        private ITransactionScope _currentTransaction;
        private bool _isDisposed;

        /// <summary>
        /// Gets the currently active transaction scope, if one exists.
        /// </summary>
        public ITransactionScope CurrentTransaction => _currentTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class with the specified database connection.
        /// </summary>
        /// <param name="connection">The database connection to be used for transactions.</param>
        public TransactionManager(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _currentTransaction = null!;
        }

        /// <summary>
        /// Begins a transaction using the provided isolation level, or returns the current active transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
        /// <returns>The active <see cref="ITransactionScope"/>.</returns>
        public ITransactionScope BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ThrowIfDisposed();

            if (_currentTransaction != null && !_currentTransaction.IsCompleted)
                return _currentTransaction;
            //throw new InvalidOperationException("A transaction is already active. Complete or dispose the current transaction before starting a new one.");

            _currentTransaction = new TransactionScope(_connection, isolationLevel);
            return _currentTransaction;
        }

        //todo: Future enhancement - support savepoints if the underlying provider supports it
        //public async Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        //{
        //    return await Task.FromResult(BeginTransaction(isolationLevel));
        //}

        /// <summary>
        /// Executes a function inside the current transaction context.
        /// </summary>
        /// <typeparam name="TResult">The return type produced by the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>The value returned by <paramref name="operation"/>.</returns>
        public TResult ExecuteInTransaction<TResult>(Func<ITransactionScope, TResult> operation)
        {
            ThrowIfDisposed();

            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            //using (var scope = BeginTransaction())
            var scope = BeginTransaction();
            try
            {
                var result = operation(scope);
                //scope.Commit();
                return result;
            }
            catch
            {
                scope.Rollback();
                throw;
            }
            finally
            {
                // _currentTransaction = null;
            }
            
        }

        //todo: Future enhancement - support savepoints if the underlying provider supports it
        //public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<ITransactionScope, Task<TResult>> operation)
        //{
        //    ThrowIfDisposed();

        //    if (operation == null)
        //        throw new ArgumentNullException(nameof(operation));

        //    using (var scope = BeginTransaction())
        //    {
        //        try
        //        {
        //            var result = await operation(scope);
        //            await scope.CommitAsync();
        //            return result;
        //        }
        //        catch
        //        {
        //            await scope.RollbackAsync();
        //            throw;
        //        }
        //        finally
        //        {
        //            _currentTransaction = null;
        //        }
        //    }
        //}


        /// <summary>
        /// Executes an action inside the current transaction context.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        public void ExecuteInTransaction(Action<ITransactionScope> operation)
        {
            ThrowIfDisposed();

            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            //using (var scope = BeginTransaction())
            var scope = BeginTransaction();
            try
            {
                operation(scope);
                //scope.Commit();
            }
            catch
            {
                scope.Rollback();
                throw;
            }
            finally
            {
                //_currentTransaction = null;
            }
            
        }

        /// <summary>
        /// Commits and disposes the current transaction, if one is active.
        /// </summary>
        public void Commit()
        {
            if (_isDisposed)
                return;

            _currentTransaction?.Commit();
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }

        /// <summary>
        /// Executes the supplied command inside a transaction and maps each returned row to a new object instance.
        /// </summary>
        /// <typeparam name="T">The target model type.</typeparam>
        /// <param name="dbCommand">The command to execute.</param>
        /// <param name="metaDatas">The metadata describing which columns to map.</param>
        /// <returns>An enumerable sequence of mapped objects.</returns>
        public IEnumerable<T> ExecuteInTransaction<T>(IDbCommand dbCommand, MetaDataInfo[] metaDatas) where T : class, new()
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            var scope = BeginTransaction();

            
                dbCommand.Transaction = scope.Transaction;
            using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    //List<T> results = new List<T>();
                    while (reader.Read())
                    {
                        T obj = new T();
                        foreach (var metaData in metaDatas)
                        {
                            var prop = props.Find(metaData.ColumnName, true);
                            if (prop != null && !reader.IsDBNull(reader.GetOrdinal(metaData.ColumnName)))
                            {
                                var value = reader.GetValue(reader.GetOrdinal(metaData.ColumnName));
                                prop.SetValue(obj, value);
                            }
                        }
                        yield return obj;
                    }
                }
            scope.Commit();
        }

        //todo: Future enhancement - support savepoints if the underlying provider supports it
        //public async Task ExecuteInTransactionAsync(Func<ITransactionScope, Task> operation)
        //{
        //    ThrowIfDisposed();

        //    if (operation == null)
        //        throw new ArgumentNullException(nameof(operation));

        //    using (var scope = BeginTransaction())
        //    {
        //        try
        //        {
        //            await operation(scope);
        //            await scope.CommitAsync();
        //        }
        //        catch
        //        {
        //            await scope.RollbackAsync();
        //            throw;
        //        }
        //        finally
        //        {
        //            _currentTransaction = null;
        //        }
        //    }
        //}

        /// <summary>
        /// Releases the active transaction and closes the underlying database connection.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _currentTransaction?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _isDisposed = true;
        }
#if NET6_0_OR_GREATER
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            if (_currentTransaction != null)
                //await _currentTransaction.DisposeAsync();
                _currentTransaction.Dispose();
            await Task.CompletedTask;

            _connection?.Close();
            _connection?.Dispose();
            _isDisposed = true;
        }
#endif

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
