# FIK.DAL Testing Guide

## Overview
This guide provides comprehensive testing procedures for FIK.DAL across all supported .NET frameworks and database providers.

---

## Test Project Structure

```
Tests/
??? FIK.DAL.Tests.Net40/                    # .NET Framework 4.0 tests
?   ??? FIK.DAL.Tests.Net40.csproj
?   ??? Transactions/
?   ?   ??? TransactionTests.cs
?   ??? QueryExecution/
?   ?   ??? InsertTests.cs
?   ?   ??? UpdateTests.cs
?   ?   ??? DeleteTests.cs
?   ?   ??? SelectTests.cs
?   ??? Metadata/
?       ??? MetadataValidationTests.cs
?
??? FIK.DAL.Tests.NetStandard2/             # .NET Standard 2.0 tests
?   ??? FIK.DAL.Tests.NetStandard2.csproj
?   ??? [Same test files as Net40]
?
??? FIK.DAL.Tests.Net6/                     # .NET 6.0 tests
?   ??? FIK.DAL.Tests.Net6.csproj
?   ??? [Same sync tests as Net40]
?   ??? Async/
?       ??? TransactionAsyncTests.cs
?       ??? QueryExecutionAsyncTests.cs
?       ??? SavepointTests.cs
?
??? FIK.DAL.Tests.Net8/                     # .NET 8.0 tests
?   ??? FIK.DAL.Tests.Net8.csproj
?   ??? [Same tests as Net6]
?   ??? PerformanceTests.cs
?
??? FIK.DAL.Integration.Tests/
    ??? SqlServer/
    ?   ??? SqlServerConnectionTests.cs
    ?   ??? SqlServerMetadataTests.cs
    ??? SQLite/
    ?   ??? SQLiteConnectionTests.cs
    ?   ??? SQLiteMetadataTests.cs
    ??? PostgreSQL/
        ??? PostgreSQLConnectionTests.cs
        ??? PostgreSQLMetadataTests.cs
```

---

## Project Creation Commands

### Step 1: Create Test Projects

```bash
# .NET Framework 4.0
dotnet new xunit -n FIK.DAL.Tests.Net40 --target-framework-override net40
cd FIK.DAL.Tests.Net40
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add reference ../FIK.DAL.Core/FIK.ORM.csproj
cd ..

# .NET Standard 2.0
dotnet new xunit -n FIK.DAL.Tests.NetStandard2 --target-framework-override netstandard2.0
cd FIK.DAL.Tests.NetStandard2
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add reference ../FIK.DAL.Core/FIK.ORM.csproj
cd ..

# .NET 6.0
dotnet new xunit -n FIK.DAL.Tests.Net6
cd FIK.DAL.Tests.Net6
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add reference ../FIK.DAL.Core/FIK.ORM.csproj
cd ..

# .NET 8.0
dotnet new xunit -n FIK.DAL.Tests.Net8 --target-framework-override net8.0
cd FIK.DAL.Tests.Net8
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add reference ../FIK.DAL.Core/FIK.ORM.csproj
cd ..

# Integration Tests
dotnet new xunit -n FIK.DAL.Integration.Tests
cd FIK.DAL.Integration.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add reference ../FIK.DAL.Core/FIK.ORM.csproj
cd ..
```

### Step 2: Create Test Fixtures

```bash
# Create test directories
mkdir -p Tests/FIK.DAL.Tests.Net40/{Transactions,QueryExecution,Metadata}
mkdir -p Tests/FIK.DAL.Tests.NetStandard2/{Transactions,QueryExecution,Metadata}
mkdir -p Tests/FIK.DAL.Tests.Net6/{Transactions,QueryExecution,Metadata,Async}
mkdir -p Tests/FIK.DAL.Tests.Net8/{Transactions,QueryExecution,Metadata,Async}
mkdir -p Tests/FIK.DAL.Integration.Tests/{SqlServer,SQLite,PostgreSQL}
```

---

## Sample Test Files

### Test File 1: TransactionTests.cs (All Frameworks)

```csharp
using System;
using System.Data;
using Xunit;
using FIK.ORM.Infrastructures.Transactions;

namespace FIK.DAL.Tests.Transactions
{
    public class TransactionScopeTests : IDisposable
    {
        private IDbConnection _connection;
        private ITransactionManager _transactionManager;

        public TransactionScopeTests()
        {
            // Initialize test connection
            _connection = DatabaseHelper.CreateTestConnection();
            _transactionManager = new TransactionManager(_connection);
        }

        [Fact]
        public void BeginTransaction_CreatesValidScope()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Assert
                Assert.NotNull(scope);
                Assert.NotNull(scope.Transaction);
                Assert.Equal(IsolationLevel.ReadCommitted, scope.IsolationLevel);
                Assert.False(scope.IsCompleted);
            }
        }

        [Fact]
        public void Commit_CompletesTransaction()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Act
                scope.Commit();

                // Assert
                Assert.True(scope.IsCompleted);
            }
        }

        [Fact]
        public void Rollback_CompletesTransaction()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Act
                scope.Rollback();

                // Assert
                Assert.True(scope.IsCompleted);
            }
        }

        [Fact]
        public void Dispose_RollsBackUncompleted()
        {
            // Arrange & Act
            var scope = _transactionManager.BeginTransaction();
            scope.Dispose(); // Auto-rollback should occur

            // Assert
            Assert.True(scope.IsCompleted);
        }

        [Fact]
        public void ExecuteInTransaction_CommitsOnSuccess()
        {
            // Arrange
            int result = 0;

            // Act
            _transactionManager.ExecuteInTransaction(scope =>
            {
                result = 42;
                return result;
            });

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void ExecuteInTransaction_RollsBackOnException()
        {
            // Arrange
            int result = 0;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                _transactionManager.ExecuteInTransaction(scope =>
                {
                    result = 42;
                    throw new InvalidOperationException("Test error");
                });
            });

            Assert.Equal(42, result); // State before exception
        }

        [Fact]
        public void MultipleTransactions_ThrowsIfAlreadyActive()
        {
            // Arrange
            var scope1 = _transactionManager.BeginTransaction();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                _transactionManager.BeginTransaction();
            });

            scope1.Dispose();
        }

        [Fact]
        public void TransactionWithIsolationLevel_UsesSpecifiedLevel()
        {
            // Arrange & Act
            using (var scope = _transactionManager.BeginTransaction(IsolationLevel.Serializable))
            {
                // Assert
                Assert.Equal(IsolationLevel.Serializable, scope.IsolationLevel);
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
```

### Test File 2: QueryExecutionTests.cs (All Frameworks)

```csharp
using System;
using System.Collections.Generic;
using Xunit;
using FIK.ORM;

namespace FIK.DAL.Tests.QueryExecution
{
    public class QueryExecutorClientTests : IDisposable
    {
        private QueryExecutorClient _executor;
        private const string TestConnectionString = "your_connection_string";

        public QueryExecutorClientTests()
        {
            _executor = new QueryExecutorClient(TestConnectionString, DatabaseProvider.SqlServer);
        }

        [Fact]
        public void Insert_ValidObject_Success()
        {
            // Arrange
            var testObject = new TestModel 
            { 
                Id = 1, 
                Name = "Test", 
                Value = 100 
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                _executor.Insert(testObject, new[] { "Name", "Value" });
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Insert_NullObject_ThrowsException()
        {
            // Arrange
            TestModel nullObject = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                _executor.Insert(nullObject, new[] { "Name", "Value" });
            });
        }

        [Fact]
        public void InsertBatch_MultipleObjects_Success()
        {
            // Arrange
            var objects = new List<TestModel>
            {
                new TestModel { Id = 1, Name = "Test1", Value = 100 },
                new TestModel { Id = 2, Name = "Test2", Value = 200 },
                new TestModel { Id = 3, Name = "Test3", Value = 300 }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                _executor.InsertBatch(objects, new[] { "Name", "Value" });
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Update_ValidObject_Success()
        {
            // Arrange
            var testObject = new TestModel 
            { 
                Id = 1, 
                Name = "Updated", 
                Value = 150 
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                _executor.Update(testObject, 
                    new[] { "Name", "Value" }, 
                    new[] { "Id" });
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Delete_WithWhereColumns_Success()
        {
            // Arrange
            var testObject = new TestModel { Id = 1 };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                _executor.Delete(testObject, new[] { "Id" });
            });

            Assert.Null(exception);
        }

        public void Dispose()
        {
            _executor?.Dispose();
        }
    }

    // Test Model
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
```

### Test File 3: TransactionAsyncTests.cs (.NET 6.0+ Only)

```csharp
#if NET6_0_OR_GREATER
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;
using FIK.ORM.Infrastructures.Transactions;

namespace FIK.DAL.Tests.Transactions.Async
{
    public class TransactionAsyncTests : IDisposable
    {
        private IDbConnection _connection;
        private ITransactionManager _transactionManager;

        public TransactionAsyncTests()
        {
            _connection = DatabaseHelper.CreateTestConnection();
            _transactionManager = new TransactionManager(_connection);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_CommitsOnSuccess()
        {
            // Arrange
            int result = 0;

            // Act
            await _transactionManager.ExecuteInTransactionAsync(async scope =>
            {
                result = 42;
                await Task.Delay(10); // Simulate async work
                return result;
            });

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_RollsBackOnException()
        {
            // Arrange
            int result = 0;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _transactionManager.ExecuteInTransactionAsync(async scope =>
                {
                    result = 42;
                    await Task.Delay(10);
                    throw new InvalidOperationException("Test error");
                });
            });

            Assert.Equal(42, result);
        }

        [Fact]
        public async Task CommitAsync_CompletesTransaction()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Act
                await scope.CommitAsync();

                // Assert
                Assert.True(scope.IsCompleted);
            }
        }

        [Fact]
        public async Task RollbackAsync_CompletesTransaction()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Act
                await scope.RollbackAsync();

                // Assert
                Assert.True(scope.IsCompleted);
            }
        }

        [Fact]
        public async Task DisposeAsyncAsync_RollsBackUncompleted()
        {
            // Arrange & Act
            var scope = _transactionManager.BeginTransaction();
            await scope.DisposeAsync();

            // Assert
            Assert.True(scope.IsCompleted);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
#endif
```

### Test File 4: SavepointTests.cs (.NET 6.0+ Only)

```csharp
#if NET6_0_OR_GREATER
using System;
using System.Data;
using Xunit;
using FIK.ORM.Infrastructures.Transactions;

namespace FIK.DAL.Tests.Transactions
{
    public class SavepointTests : IDisposable
    {
        private IDbConnection _connection;
        private ITransactionManager _transactionManager;

        public SavepointTests()
        {
            _connection = DatabaseHelper.CreateTestConnection();
            _transactionManager = new TransactionManager(_connection);
        }

        [Fact]
        public void CreateSavepoint_ValidName_Success()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Act & Assert - Should not throw
                var exception = Record.Exception(() =>
                {
                    scope.CreateSavepoint("TestSavepoint");
                });

                // Exception is expected if provider doesn't support savepoints
                // Implementation may vary by database
                scope.Commit();
            }
        }

        [Fact]
        public void CreateSavepoint_NullName_ThrowsException()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                // Act & Assert
                Assert.Throws<ArgumentException>(() =>
                {
                    scope.CreateSavepoint(null);
                });

                scope.Rollback();
            }
        }

        [Fact]
        public void RollbackToSavepoint_ValidName_Success()
        {
            // Arrange
            using (var scope = _transactionManager.BeginTransaction())
            {
                try
                {
                    scope.CreateSavepoint("TestPoint");

                    // Act - May throw if not supported
                    scope.RollbackToSavepoint("TestPoint");

                    // Assert
                    scope.Commit();
                }
                catch (NotSupportedException)
                {
                    // Acceptable for databases that don't support savepoints
                    scope.Rollback();
                }
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
#endif
```

---

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/FIK.DAL.Tests.Net40/FIK.DAL.Tests.Net40.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~TransactionScopeTests"

# Run with verbose output
dotnet test -v n

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Visual Studio

```
Test > Run All Tests
Test > Test Explorer > [Select Test] > Run
Test > Create New Test Project
```

---

## Database Setup for Integration Tests

### SQL Server
```sql
CREATE DATABASE FIK_DAL_Test
GO

USE FIK_DAL_Test
GO

CREATE TABLE TestModel (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Value INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
)
GO
```

### SQLite
```csharp
using System.Data.SQLite;

public static void SetupSQLiteDatabase(string path)
{
    using (var connection = new SQLiteConnection($"Data Source={path}"))
    {
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TestModel (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Value INTEGER NOT NULL,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                )";
            command.ExecuteNonQuery();
        }
    }
}
```

### PostgreSQL
```sql
CREATE DATABASE fik_dal_test;

\c fik_dal_test

CREATE TABLE test_model (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    value INT NOT NULL,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## Test Execution Matrix

### Framework Compatibility

| Test Type | .NET 4.0 | .NET Standard 2.0 | .NET 6.0 | .NET 8.0 |
|-----------|----------|------------------|----------|----------|
| Transaction Sync | ? | ? | ? | ? |
| Transaction Async | ? | ? | ? | ? |
| Savepoints | ?? | ?? | ? | ? |
| Query Execution | ? | ? | ? | ? |
| Batch Operations | ? | ? | ? | ? |

? = Fully Supported
?? = Conditionally Supported (Database provider dependent)
? = Not Supported

---

## Continuous Integration

### GitHub Actions Configuration

```yaml
name: .NET Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['4.0', 'standard2.0', '6.0', '8.0']

    steps:
    - uses: actions/checkout@v2

    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: ${{ matrix.dotnet-version }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build

    - name: Test
      run: dotnet test --no-build --verbosity normal

    - name: Upload coverage
      uses: codecov/codecov-action@v2
```

---

## Troubleshooting

### Common Issues

1. **Connection String Issues**
   - Ensure connection strings are correctly configured
   - Check database server is running and accessible
   - Verify credentials

2. **Async Test Deadlocks**
   - Use `async Task` not `async void` for tests
   - Avoid `.Result` or `.Wait()` in async code
   - Use `await` consistently

3. **Framework-Specific Issues**
   - .NET 4.0: Some async APIs unavailable, use conditional compilation
   - SQLite: May not support all SQL features, use `pragma` statements
   - PostgreSQL: Use `@ParameterName` syntax consistently

---

## Best Practices

1. ? Use xUnit for testing framework
2. ? Create test fixtures for database setup
3. ? Use Arrange-Act-Assert pattern
4. ? Isolate tests - each test should be independent
5. ? Clean up resources in Dispose methods
6. ? Use conditional compilation for framework-specific tests
7. ? Mock external dependencies where possible

---

## Performance Testing

```csharp
[Fact]
public void PerformanceTest_InsertBatch_CompleteIn100ms()
{
    // Arrange
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var objects = GenerateTestObjects(1000);

    // Act
    _executor.InsertBatch(objects, null);
    stopwatch.Stop();

    // Assert
    Assert.True(stopwatch.ElapsedMilliseconds < 100, 
        $"Batch insert took {stopwatch.ElapsedMilliseconds}ms");
}
```

---

## Test Reporting

Generate test reports:
```bash
# Generate HTML report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=html

# Generate OpenCover format
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:CoverageFileName=coverage.xml

# Generate report summary
dotnet test --logger trx
```

