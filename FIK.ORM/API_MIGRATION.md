# API Migration Guide v1.9 ? v2.0

## Overview
This guide helps you migrate your code from FIK.DAL v1.9 to v2.0, which includes significant API improvements and breaking changes.

---

## Summary of Changes

### Breaking Changes
| Aspect | v1.9 | v2.0 | Migration Effort |
|--------|------|------|------------------|
| Error Handling | ref string ErrorMsg | Exceptions | High |
| Column Selection | String "Col1,Col2" | IEnumerable<string> | Medium |
| Return Types | bool + ref error | void (throws) | High |
| Transaction Mgmt | Manual | ITransactionManager | High |
| Method Names | ExecuteQuery | ExecuteInTransaction | Low |
| Parameter Names | Typos (ExlcudeAutogenerate) | Fixed | Low |

### Backward Compatibility
- ? Not backward compatible
- ?? Major API redesign in v2.0
- ? Migration tools/scripts available
- ? Detailed examples provided

---

## Migration Path

### Phase 1: Update Error Handling (Priority: HIGH)

#### Pattern 1: Simple Insert

**Before (v1.9):**
```csharp
SQL sql = new SQL(_connectionString);
string error = "";
bool success = sql.Insert<User>(
    new User { Name = "John", Email = "john@example.com" },
    "Name,Email",
    "Id",  // ExcludeAutogeneratePrimaryKey (typo in original)
    "",    // customTable
    ref error
);

if (!success)
{
    Console.WriteLine($"Error: {error}");
}
```

**After (v2.0):**
```csharp
var executor = new QueryExecutorClient(_connectionString, DatabaseProvider.SqlServer);
try
{
    executor.Insert(
        new User { Name = "John", Email = "john@example.com" },
        new[] { "Name", "Email" },
        tableName: "",
        schemaName: "dbo"
    );
    Console.WriteLine("Insert successful");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

**Migration Steps:**
1. Replace `SQL` instance with `QueryExecutorClient`
2. Remove `ref string error` parameter
3. Wrap call in try-catch block
4. Change column string to array/enumerable
5. Remove unnecessary parameters (customTable, ExcludeAutogeneratePrimaryKey)

#### Pattern 2: Batch Insert

**Before (v1.9):**
```csharp
var users = new List<User>
{
    new User { Name = "John", Email = "john@example.com" },
    new User { Name = "Jane", Email = "jane@example.com" }
};

string error = "";
bool success = sql.Insert<User>(
    users,                    // Can be List<T> or single T
    "Name,Email",             // Comma-separated columns
    "Id",                      // Exclude auto-generated
    "",
    ref error
);

if (!success)
    MessageBox.Show($"Error: {error}");
```

**After (v2.0):**
```csharp
var users = new List<User>
{
    new User { Name = "John", Email = "john@example.com" },
    new User { Name = "Jane", Email = "jane@example.com" }
};

try
{
    executor.InsertBatch(
        users,
        new[] { "Name", "Email" }
    );
    MessageBox.Show("Batch insert successful");
}
catch (Exception ex)
{
    MessageBox.Show($"Error: {ex.Message}");
}
```

**Key Differences:**
- Use `InsertBatch()` explicitly for collections
- Use `Insert()` for single objects only
- Pass string array instead of comma-separated string

#### Pattern 3: Update Operation

**Before (v1.9):**
```csharp
string error = "";
bool success = sql.Update<User>(
    user,                      // Updated object
    "Name,Email",              // SET columns
    "Id",                       // WHERE columns
    "",
    ref error
);

if (!success)
    Console.WriteLine(error);
```

**After (v2.0):**
```csharp
try
{
    executor.Update(
        user,
        new[] { "Name", "Email" },  // SET columns
        new[] { "Id" }               // WHERE columns
    );
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

#### Pattern 4: Delete Operation

**Before (v1.9):**
```csharp
string error = "";
bool success = sql.Delete<User>(
    user,           // Object with WHERE values
    "Id",           // WHERE columns
    "",
    ref error
);

if (!success)
    Console.WriteLine(error);
```

**After (v2.0):**
```csharp
try
{
    executor.Delete(
        user,
        new[] { "Id" }  // WHERE columns
    );
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

---

### Phase 2: Update Column Selection (Priority: HIGH)

#### String-based Columns (OLD) ? Array/Enumerable (NEW)

**Pattern Evolution:**

```csharp
// v1.9: String with commas
"Name,Email,Phone"
"Col1,Col2,Col3"

// v2.0: String array
new[] { "Name", "Email", "Phone" }

// v2.0: List<string>
new List<string> { "Name", "Email", "Phone" }

// v2.0: IEnumerable<string>
list.Where(x => x.IsActive).Select(x => x.ColumnName)
```

**Conversion Utility:**
```csharp
public static class MigrationHelpers
{
    /// <summary>
    /// Convert v1.9 comma-separated columns to v2.0 string array
    /// </summary>
    public static string[] ParseColumns(string columnString)
    {
        if (string.IsNullOrEmpty(columnString))
            return Array.Empty<string>();

        return columnString
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
    }

    /// <summary>
    /// Safe conversion with null check
    /// </summary>
    public static IEnumerable<string> ToColumnArray(string columnString)
    {
        return ParseColumns(columnString) ?? Enumerable.Empty<string>();
    }
}

// Usage:
var columns = MigrationHelpers.ParseColumns("Name,Email,Phone");
executor.Insert(user, columns);
```

**Batch Conversion Script:**
```csharp
// Find and replace patterns (use in Find/Replace dialog)

// Pattern 1: Single quotes
Find:    Insert<(\w+)>\([^,]+,\s*"([^"]+)"
Replace: Insert<$1>(..., new[] { "COLUMNS_HERE" }

// Pattern 2: Columns with variable
Find:    (\w+),\s*ref error\)
Replace: ParseColumns($1))
```

---

### Phase 3: Update Exception Handling (Priority: CRITICAL)

#### Pattern: Try-Catch-Finally

**Before (v1.9):**
```csharp
public void ProcessUser(User user)
{
    SQL sql = new SQL(_connectionString);
    string error = "";

    try
    {
        if (!sql.Insert<User>(user, "Name", "", "", ref error))
        {
            Logger.Error($"Insert failed: {error}");
            return;
        }

        if (!sql.Update<User>(user, "Email", "Id", "", ref error))
        {
            Logger.Error($"Update failed: {error}");
            return;
        }

        Logger.Info("Success");
    }
    catch (Exception ex)
    {
        Logger.Error($"Unexpected error: {ex.Message}");
    }
    finally
    {
        sql?.Dispose();
    }
}
```

**After (v2.0):**
```csharp
public void ProcessUser(User user)
{
    var executor = new QueryExecutorClient(_connectionString, DatabaseProvider.SqlServer);

    try
    {
        executor.Insert(user, new[] { "Name" });
        executor.Update(user, new[] { "Email" }, new[] { "Id" });
        Logger.Info("Success");
    }
    catch (ArgumentException ex)
    {
        Logger.Error($"Invalid argument: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Logger.Error($"Operation failed: {ex.Message}");
    }
    catch (Exception ex)
    {
        Logger.Error($"Unexpected error: {ex.Message}");
    }
}
```

**Exception Types to Handle:**

| Exception Type | Cause | Action |
|----------------|-------|--------|
| `ArgumentException` | Null/empty parameters | Validate input |
| `ArgumentNullException` | Parameter is null | Check null |
| `InvalidOperationException` | Database operation failed | Log and retry |
| `NotSupportedException` | Feature not supported by provider | Use alternative |
| `Exception` | Unknown error | Log and notify |

---

### Phase 4: Transaction Management (Priority: HIGH)

#### Simple Transaction

**Before (v1.9):**
```csharp
List<string> queries = new List<string>
{
    "INSERT INTO Users (Name) VALUES ('John')",
    "INSERT INTO Users (Name) VALUES ('Jane')"
};

string error = "";
bool success = sql.ExecuteQuery(queries, ref error);

if (!success)
    Console.WriteLine($"Transaction failed: {error}");
```

**After (v2.0):**
```csharp
var executor = new QueryExecutorClient(_connectionString, DatabaseProvider.SqlServer);

try
{
    executor.ExecuteQueryBatch(new[]
    {
        "INSERT INTO Users (Name) VALUES ('John')",
        "INSERT INTO Users (Name) VALUES ('Jane')"
    });
}
catch (Exception ex)
{
    Console.WriteLine($"Transaction failed: {ex.Message}");
}
```

#### Explicit Transaction Control

**New in v2.0:**
```csharp
try
{
    // Access transaction manager
    var transactionManager = executor.GetTransactionManager();

    transactionManager.ExecuteInTransaction(scope =>
    {
        // All operations here are in same transaction
        executor.Insert(user1, columns);
        executor.Insert(user2, columns);

        // Auto-commit on success
        // Auto-rollback on exception
        return true;
    });
}
catch (Exception ex)
{
    // Transaction already rolled back
    Console.WriteLine($"Error: {ex.Message}");
}
```

#### Savepoint Support (.NET 6+ only)

**New in v2.0:**
```csharp
#if NET6_0_OR_GREATER
try
{
    var transactionManager = executor.GetTransactionManager();

    var scope = transactionManager.BeginTransaction();
    using (scope)
    {
        executor.Insert(user1, columns);
        scope.CreateSavepoint("after_insert_1");

        executor.Insert(user2, columns);

        if (ValidationFailed)
        {
            scope.RollbackToSavepoint("after_insert_1");
        }

        scope.Commit();
    }
}
catch (NotSupportedException)
{
    // Savepoints not supported by this provider
}
#endif
```

---

### Phase 5: Method Signature Changes (Priority: MEDIUM)

#### Parameter Name Fixes

**Before (v1.9):**
```csharp
// Typo: "Exlcude" instead of "Exclude"
public bool Insert<T>(
    object dataObject,
    string specificProperty,
    string ExlcudeAutogeneratePrimaryKey,  // ?? TYPO
    string customTable,
    ref string ErrorMsg)
```

**After (v2.0):**
```csharp
// All parameters now have clear names
public void Insert<T>(
    T dataObject,
    IEnumerable<string>? columns,
    string tableName = "",
    string schemaName = "dbo") where T : class
```

**Mapping:**
| v1.9 | v2.0 | Notes |
|------|------|-------|
| dataObject | dataObject | Same purpose, type changed (object ? T) |
| specificProperty | columns | Renamed for clarity |
| ExlcudeAutogeneratePrimaryKey | ? Removed | Handled by metadata validator |
| customTable | tableName | Renamed, optional parameter |
| (new) | schemaName | New parameter, defaults to "dbo" |
| ErrorMsg (ref) | ? Removed | Replaced with exceptions |

---

### Phase 6: Composite Model Changes (Priority: MEDIUM)

#### CompositeModelBuilder Replacement

**Before (v1.9):**
```csharp
var compositeModel = new CompositeModelBuilder();
compositeModel.AddUserWithOrders(user, orders);

string error = "";
bool success = sql.Insert<CompositeModel>(
    compositeModel,
    "UserId,OrderId",
    "",
    "",
    ref error
);
```

**After (v2.0):**
```csharp
// Use CompositeModelBuilder for object composition
var compositeModel = new CompositeModelBuilder();
var combined = compositeModel.AddUserWithOrders(user, orders);

try
{
    // Then insert normally
    executor.Insert(combined, new[] { "UserId", "OrderId" });
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

---

## Automated Migration Script

### C# Migration Helper

```csharp
/// <summary>
/// Helps migrate code from v1.9 to v2.0
/// </summary>
public class V19ToV20Migrator
{
    public static class ColumnConverter
    {
        /// <summary>
        /// Convert "Col1,Col2" to new[] { "Col1", "Col2" }
        /// </summary>
        public static string[] Parse(string v19Columns)
        {
            if (string.IsNullOrEmpty(v19Columns))
                return Array.Empty<string>();

            return v19Columns
                .Split(',')
                .Select(x => x.Trim().Trim('"', '\''))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
        }
    }

    public static class ErrorHandling
    {
        /// <summary>
        /// Convert v1.9 error pattern to v2.0
        /// </summary>
        public static void MigrateErrorHandling(
            Action operation,
            Action<Exception> onError)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }
    }

    public static class TransactionHelper
    {
        /// <summary>
        /// Migrate batch queries to new transaction model
        /// </summary>
        public static void ExecuteMigratedQueries(
            QueryExecutorClient executor,
            IEnumerable<string> queries)
        {
            var transactionManager = executor.GetTransactionManager();
            transactionManager.ExecuteInTransaction(scope =>
            {
                foreach (var query in queries)
                {
                    // Execute each query in same transaction
                    executor.ExecuteQueryBatch(new[] { query });
                }
            });
        }
    }
}

// Usage:
var columns = V19ToV20Migrator.ColumnConverter.Parse("Name,Email,Phone");
V19ToV20Migrator.ErrorHandling.MigrateErrorHandling(
    () => executor.Insert(user, columns),
    ex => Console.WriteLine(ex.Message)
);
```

---

## Testing Migration

### Unit Test Update

**Before (v1.9):**
```csharp
[TestMethod]
public void TestInsert()
{
    string error = "";
    bool result = sql.Insert<User>(user, "Name", "Id", "", ref error);

    Assert.IsTrue(result);
    Assert.IsTrue(string.IsNullOrEmpty(error));
}
```

**After (v2.0):**
```csharp
[Fact]
public void TestInsert_ValidUser_Success()
{
    // Arrange
    var user = new User { Name = "John" };

    // Act & Assert
    var exception = Record.Exception(() =>
    {
        executor.Insert(user, new[] { "Name" });
    });

    Assert.Null(exception);
}

[Fact]
public void TestInsert_NullUser_ThrowsException()
{
    // Arrange
    User nullUser = null;

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
    {
        executor.Insert(nullUser, new[] { "Name" });
    });
}
```

---

## Framework-Specific Considerations

### .NET Framework 4.0
```csharp
// ? All sync methods available
executor.Insert(user, columns);
executor.Update(user, columns, whereColumns);

// ? No async support
// await executor.InsertAsync(...);  // Not available

// ? No IAsyncDisposable
// await executor.DisposeAsync();    // Not available
```

### .NET 6.0+
```csharp
// ? All sync methods available
executor.Insert(user, columns);

// ? Async support
await executor.InsertAsync(user, columns);

// ? IAsyncDisposable
await using (executor) { }

// ? Savepoints
scope.CreateSavepoint("point1");
```

---

## Common Migration Issues

### Issue 1: Column Type Mismatch

**Problem:**
```csharp
// Passing wrong type
string columns = "Name,Email";
executor.Insert(user, columns);  // ? Expects IEnumerable<string>
```

**Solution:**
```csharp
executor.Insert(user, new[] { "Name", "Email" });  // ?
// Or
executor.Insert(user, "Name,Email".Split(','));     // ?
```

### Issue 2: Missing Schema Parameter

**Problem:**
```csharp
// SQLite doesn't use schemas but still required in v2.0
executor.Insert(user, columns, "Users");  // ? Missing schemaName
```

**Solution:**
```csharp
// Use explicit parameter names
executor.Insert(
    user,
    columns,
    tableName: "Users",
    schemaName: "main"  // or empty string for SQLite
);
```

### Issue 3: Exception Not Caught

**Problem:**
```csharp
executor.Insert(user, columns);  // ? Exception not caught
```

**Solution:**
```csharp
try
{
    executor.Insert(user, columns);
}
catch (Exception ex)
{
    Logger.Error(ex);
}
```

---

## Validation Checklist

Before deploying v2.0 migration:

- [ ] All Insert/Update/Delete methods updated
- [ ] Error handling uses try-catch (not ref string)
- [ ] Column selection uses IEnumerable<string> (not comma string)
- [ ] Transaction code uses ExecuteInTransaction
- [ ] All string column specifications converted
- [ ] Tests updated to use new API
- [ ] No references to ref ErrorMsg remain
- [ ] CompositeModel usage checked
- [ ] Framework-specific code verified
- [ ] All builds passing on all targets

---

## Quick Reference: Side-by-Side

```csharp
// INSERT
// v1.9
sql.Insert<User>(user, "Name,Email", "Id", "", ref error);

// v2.0
executor.Insert(user, new[] { "Name", "Email" });

// UPDATE
// v1.9
sql.Update<User>(user, "Email", "Id", "", ref error);

// v2.0
executor.Update(user, new[] { "Email" }, new[] { "Id" });

// DELETE
// v1.9
sql.Delete<User>(user, "Id", "", ref error);

// v2.0
executor.Delete(user, new[] { "Id" });

// BATCH
// v1.9
sql.Insert<User>(userList, "Name,Email", "Id", "", ref error);

// v2.0
executor.InsertBatch(userList, new[] { "Name", "Email" });
```

---

## Support & Questions

- ?? See MODERNIZATION_SUMMARY.md for overview
- ?? See TESTING_GUIDE.md for testing
- ?? See GIT_WORKFLOW.md for git procedures
- ?? Report issues on GitHub
- ?? Ask questions in discussions

