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

        public ITransactionScope CurrentTransaction => _currentTransaction;

        public TransactionManager(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public ITransactionScope BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ThrowIfDisposed();

            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already active. Complete or dispose the current transaction before starting a new one.");

            _currentTransaction = new TransactionScope(_connection, isolationLevel);
            return _currentTransaction;
        }

        //todo: Future enhancement - support savepoints if the underlying provider supports it
        //public async Task<ITransactionScope> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        //{
        //    return await Task.FromResult(BeginTransaction(isolationLevel));
        //}

        public TResult ExecuteInTransaction<TResult>(Func<ITransactionScope, TResult> operation)
        {
            ThrowIfDisposed();

            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var scope = BeginTransaction())
            {
                try
                {
                    var result = operation(scope);
                    scope.Commit();
                    return result;
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
                finally
                {
                    _currentTransaction = null;
                }
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

        public void ExecuteInTransaction(Action<ITransactionScope> operation)
        {
            ThrowIfDisposed();

            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var scope = BeginTransaction())
            {
                try
                {
                    operation(scope);
                    scope.Commit();
                }
                catch
                {
                    scope.Rollback();
                    throw;
                }
                finally
                {
                    _currentTransaction = null;
                }
            }
        }

        public IEnumerable<T> ExecuteInTransaction<T>(IDbConnection iDbConnection, IDbCommand oCmd, MetaDataInfo[] metaDatas) where T : class, new()
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            using (IDataReader reader = oCmd.ExecuteReader())
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

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _currentTransaction?.Dispose();
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
