using FIK.ORM.Infrastructures.Transactions;
using NUnit.Framework;
using System;
using System.Configuration;
using System.Data;

namespace FIK.ORM.Tests.Net40
{
    [TestFixture]
    public class TransactionManagerTests
    {
        [Test]
        public void BeginTransaction_CreatesActiveScope()
        {
            var connection = new FakeDbConnection();
            var manager = new TransactionManager(connection);

            using (var scope = manager.BeginTransaction())
            {
                Assert.That(scope, Is.Not.Null);
                Assert.That(scope.IsCompleted, Is.False);
                Assert.That(connection.OpenCallCount, Is.EqualTo(1));
                Assert.That(connection.BeginTransactionCallCount, Is.EqualTo(1));
                Assert.That(connection.ConnectionString, Is.EqualTo(TestSettings.ConnectionString));
            }
        }

        [Test]
        public void ExecuteInTransaction_CommitsSuccessfulOperation()
        {
            var connection = new FakeDbConnection();
            var manager = new TransactionManager(connection);
            var result = manager.ExecuteInTransaction(scope => 42);
            manager.Commit();

            Assert.That(result, Is.EqualTo(42));
            Assert.That(connection.LastTransaction.CommitCallCount, Is.EqualTo(1));
            Assert.That(connection.LastTransaction.RollbackCallCount, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteInTransaction_RollsBackWhenOperationThrows()
        {
            var connection = new FakeDbConnection();
            var manager = new TransactionManager(connection);

            Assert.Throws<InvalidOperationException>(() =>
                manager.ExecuteInTransaction(scope =>
                {
                    throw new InvalidOperationException("boom");
                }));

            Assert.That(connection.LastTransaction.CommitCallCount, Is.EqualTo(0));
            Assert.That(connection.LastTransaction.RollbackCallCount, Is.EqualTo(1));
        }

        [Test]
        public void FakeConnection_ReadsConnectionStringFromAppConfig()
        {
            var connection = new FakeDbConnection();

            Assert.That(connection.ConnectionString, Is.EqualTo(TestSettings.ConnectionString));
            Assert.That(TestSettings.DatabaseProvider, Is.EqualTo("SqlServer"));
        }

        

        private sealed class FakeDbConnection : IDbConnection
        {
            public FakeDbConnection()
            {
                ConnectionString = TestSettings.ConnectionString;
            }

            public int OpenCallCount { get; private set; }
            public int BeginTransactionCallCount { get; private set; }
            public FakeDbTransaction LastTransaction { get; private set; }

            public string ConnectionString { get; set; }
            public int ConnectionTimeout { get { return 30; } }
            public string Database { get { return "Fake"; } }
            public ConnectionState State { get; private set; }

            public IDbTransaction BeginTransaction()
            {
                return BeginTransaction(IsolationLevel.ReadCommitted);
            }

            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                BeginTransactionCallCount++;
                LastTransaction = new FakeDbTransaction(this, il);
                return LastTransaction;
            }

            public void ChangeDatabase(string databaseName)
            {
            }

            public void Close()
            {
                State = ConnectionState.Closed;
            }

            public IDbCommand CreateCommand()
            {
                throw new NotSupportedException();
            }

            public void Open()
            {
                OpenCallCount++;
                State = ConnectionState.Open;
            }

            public void Dispose()
            {
                Close();
            }
        }

        private sealed class FakeDbTransaction : IDbTransaction
        {
            private readonly FakeDbConnection _connection;

            public FakeDbTransaction(FakeDbConnection connection, IsolationLevel isolationLevel)
            {
                _connection = connection;
                IsolationLevel = isolationLevel;
            }

            public int CommitCallCount { get; private set; }
            public int RollbackCallCount { get; private set; }
            public IDbConnection Connection { get { return _connection; } }
            public IsolationLevel IsolationLevel { get; private set; }

            public void Commit()
            {
                CommitCallCount++;
            }

            public void Rollback()
            {
                RollbackCallCount++;
            }

            public void Dispose()
            {
            }
        }
    }
}
