using FIK.ORM.Infrastructures.Transactions;
using NUnit.Framework;
using System;
using System.Configuration;
using System.Data;

namespace FIK.ORM.Tests.Net40.Classic
{
    [TestFixture]
    public class TransactionManagerClassicTests
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
        public void FakeConnection_ReadsConnectionStringFromAppConfig()
        {
            var connection = new FakeDbConnection();

            Assert.That(connection.ConnectionString, Is.EqualTo(TestSettings.ConnectionString));
            Assert.That(TestSettings.DatabaseProvider, Is.EqualTo("SqlServer"));
        }

        private static class TestSettings
        {
            public static string ConnectionString
            {
                get
                {
                    var entry = ConfigurationManager.ConnectionStrings["TestDatabase"];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.ConnectionString))
                    {
                        throw new InvalidOperationException("Missing 'TestDatabase' connection string in App.config.");
                    }

                    return entry.ConnectionString;
                }
            }

            public static string DatabaseProvider
            {
                get
                {
                    var provider = ConfigurationManager.AppSettings["DatabaseProvider"];
                    if (string.IsNullOrWhiteSpace(provider))
                    {
                        throw new InvalidOperationException("Missing 'DatabaseProvider' appSetting in App.config.");
                    }

                    return provider;
                }
            }
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

            public IDbConnection Connection { get { return _connection; } }
            public IsolationLevel IsolationLevel { get; private set; }

            public void Commit()
            {
            }

            public void Rollback()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
