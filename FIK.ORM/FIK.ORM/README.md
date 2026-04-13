# FIK.ORM - Lightweight ORM for .NET

A lightweight, database-agnostic Object-Relational Mapping (ORM) library for .NET that simplifies CRUD operations and batch processing across multiple database providers including SQL Server, SQLite, and PostgreSQL.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Supported Databases](#supported-databases)
- [Supported Frameworks](#supported-frameworks)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [UPSERT Operations](#upsert-operations)
- [Transaction Management](#transaction-management)
- [Best Practices](#best-practices)

---

## Overview

FIK.ORM is designed to provide a simple, intuitive interface for performing database operations without writing complex SQL queries. It automatically generates and executes appropriate SQL for the target database provider, handling dialect differences transparently.

### What FIK.ORM Does

Behind the scenes, FIK.ORM:

1. **Analyzes your data model** - Inspects .NET class properties and types
2. **Generates database-specific SQL** - Creates optimized queries for SQL Server, SQLite, or PostgreSQL
3. **Manages transactions** - Ensures data consistency across operations
4. **Handles parameter binding** - Prevents SQL injection by using parameterized queries
5. **Maps results to objects** - Converts database rows back into strongly-typed C# objects
6. **Manages schemas** - Supports custom schemas (SQL Server/PostgreSQL) or ignores them (SQLite)

---

## Features

✅ **Multi-Database Support** - SQL Server, SQLite, PostgreSQL  
✅ **CRUD Operations** - Create, Read, Update, Delete with ease  
✅ **Batch Operations** - Insert, Update, Delete multiple records efficiently  
✅ **UPSERT (Insert or Update)** - Handle insert-if-new, update-if-exists patterns  
✅ **Transaction Support** - Automatic transaction management with rollback on error  
✅ **Parameterized Queries** - Built-in SQL injection protection  
✅ **Schema Support** - Multi-schema operations for SQL Server and PostgreSQL  
✅ **Blob Support** - Handle binary data (images, files, etc.)  
✅ **Flexible Queries** - WHERE, ORDER BY, and LIMIT support  
✅ **Type-Safe** - Full IntelliSense support with strongly-typed results  

---

## Supported Databases

| Database | Version | Status |
|----------|---------|--------|
| SQL Server | 2012+ | ✅ Fully Supported |
| SQLite | 3.x | ✅ Fully Supported |
| PostgreSQL | 9.5+ | ✅ Fully Supported |

---

## Supported Frameworks

| Framework | Status |
|-----------|--------|
| .NET Framework 4.0 | ✅ Supported |
| .NET Standard 2.0 | ✅ Supported |
| .NET 6.0 | ✅ Supported |
| .NET 8.0 | ✅ Supported |

---

## Installation

### Via NuGet Package Manager

```bash
Install-Package FIK.ORM
```

### Via dotnet CLI

```bash
dotnet add package FIK.ORM
```

---

## Quick Start

### 1. Define Your Model

```csharp
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 2. Initialize the Client

```csharp
// SQL Server
var client = new QueryExecutorClient(
    connectionString, 
    DatabaseProvider.SqlServer
);

// SQLite
var client = new QueryExecutorClient(
    connectionString, 
    DatabaseProvider.Sqlite
);

// PostgreSQL
var client = new QueryExecutorClient(
    connectionString, 
    DatabaseProvider.PostgreSQL
);
```

### 3. Perform Operations

```csharp
// Create
var item = new Item { Id = 1, Name = "Widget", Price = 9.99m };
client.Insert(item, null, "Item", "dbo");
client.CommitTransaction();

// Read
var result = client.Select<Item>(
    typeof(Item), 
    null,  // all columns
    new Dictionary<string, string> { { "Id", "1" } },  // WHERE Id = 1
    null,  // no order by
    null   // no limit
).SingleOrDefault();

// Update
item.Price = 12.99m;
client.Update(item, null, new string[] { "Id" }, "Item", "dbo");
client.CommitTransaction();

// Delete
client.Delete(item, new string[] { "Id" }, "Item", "dbo");
client.CommitTransaction();
```

---

## Core Concepts

### DatabaseProvider Enum

Specifies which database to use:

```csharp
DatabaseProvider.SqlServer    // SQL Server
DatabaseProvider.Sqlite       // SQLite
DatabaseProvider.PostgreSQL   // PostgreSQL
```

### Connection Strings

```csharp
// SQL Server
"Server=localhost;Database=MyDB;Integrated Security=true;"

// SQLite
"Data Source=myDatabase.db;Version=3;"

// PostgreSQL
"Host=localhost;Username=postgres;Password=pwd;Database=mydb;"
```

### Schema Names

- **SQL Server**: Use schema names like `"dbo"`, `"sales"`, etc.
- **SQLite**: Schema names are ignored (leave empty or provide any value)
- **PostgreSQL**: Use schema names like `"public"`, `"custom_schema"`, etc.

### Column Filtering

- **Insert specific columns**: Pass column names array
- **Include all columns**: Pass `null`
- **Select specific columns**: Pass column names array

---

## API Reference

### Insert Operations

#### Insert Single Record

```csharp
void Insert<T>(
    T dataObject,                    // Object to insert
    IEnumerable<string>? columns,    // Columns to insert (null = all)
    string tableName = "",           // Table name (inferred from type if empty)
    string schemaName = "dbo"        // Schema name
) where T : class
```

#### Insert Multiple Records

```csharp
void InsertBatch<T>(
    List<T> dataObjects,             // List of objects to insert
    IEnumerable<string>? columns,    // Columns to insert (null = all)
    string tableName = "",           // Table name
    string schemaName = "dbo"        // Schema name
) where T : class
```

### Update Operations

#### Update Single Record

```csharp
void Update<T>(
    T dataObject,                    // Object with updated values
    IEnumerable<string>? columns,    // Columns to update (null = all)
    string[]? whereColumns,          // Columns for WHERE clause
    string tableName = "",           // Table name
    string schemaName = "dbo"        // Schema name
) where T : class
```

#### Update Multiple Records

```csharp
void UpdateBatch<T>(
    List<T> dataObjects,             // Objects to update
    IEnumerable<string>? columns,    // Columns to update
    string[]? whereColumns,          // WHERE clause columns
    string tableName = "",           // Table name
    string schemaName = "dbo"        // Schema name
) where T : class
```

### Delete Operations

#### Delete Single Record

```csharp
void Delete<T>(
    T dataObject,                    // Object to delete
    string[] whereColumns,           // Columns for WHERE clause
    string tableName = "",           // Table name
    string schemaName = "dbo"        // Schema name
) where T : class
```

#### Delete Multiple Records

```csharp
void DeleteBatch<T>(
    List<T> dataObjects,             // Objects to delete
    string[] whereColumns,           // Columns for WHERE clause
    string tableName = "",           // Table name
    string schemaName = "dbo"        // Schema name
) where T : class
```

### Select Operations

#### Select with Dictionary Where Clause

```csharp
IEnumerable<T> Select<T>(
    Type entityType,
    string[]? columns = null,                           // Columns to select
    Dictionary<string, string>? whereColumns = null,    // WHERE conditions
    Dictionary<string, string>? orderByColumn = null,   // ORDER BY
    int? limit = null,                                  // LIMIT
    string tableName = "",
    string schemaName = "dbo"
) where T : class, new()
```

#### Select with String Where Clause

```csharp
IEnumerable<T> Select<T>(
    Type entityType,
    string[]? columns = null,
    string whereClause = null,                          // Raw WHERE clause
    Dictionary<string, string>? orderByColumn = null,
    int? limit = null,
    string tableName = "",
    string schemaName = "dbo"
) where T : class, new()
```

### Transaction Management

```csharp
// Commit current transaction
void CommitTransaction();
```

---

## Usage Examples

### Example 1: Simple Insert

```csharp
var client = new QueryExecutorClient(
    "Server=localhost;Database=MyDB;Integrated Security=true;",
    DatabaseProvider.SqlServer
);

var item = new Item 
{ 
    Id = 1, 
    Name = "Laptop", 
    Price = 999.99m 
};

client.Insert(item, null, "Items", "dbo");
client.CommitTransaction();
```

### Example 2: Batch Insert

```csharp
var items = new List<Item>
{
    new Item { Id = 1, Name = "Item 1", Price = 10.00m },
    new Item { Id = 2, Name = "Item 2", Price = 20.00m },
    new Item { Id = 3, Name = "Item 3", Price = 30.00m }
};

client.InsertBatch(items, null, "Items", "dbo");
client.CommitTransaction();
```

### Example 3: Select with Filtering

```csharp
// Find item by ID
var whereColumns = new Dictionary<string, string> 
{ 
    { "Id", "1" } 
};

var item = client.Select<Item>(
    typeof(Item),
    null,              // all columns
    whereColumns,      // WHERE Id = 1
    null,
    null,
    "Items",
    "dbo"
).SingleOrDefault();

Console.WriteLine($"Found: {item.Name} - ${item.Price}");
```

### Example 4: Select with Multiple Conditions

```csharp
var whereClause = " WHERE Price > 10 AND Price < 100 ";

var items = client.Select<Item>(
    typeof(Item),
    null,                    // all columns
    whereClause,             // custom WHERE clause
    null,
    null,
    "Items",
    "dbo"
).ToList();

Console.WriteLine($"Found {items.Count} items");
```

### Example 5: Select with Ordering and Limit

```csharp
var orderBy = new Dictionary<string, string> 
{ 
    { "Price", "DESC" } 
};

var topItems = client.Select<Item>(
    typeof(Item),
    null,
    null,           // no WHERE clause
    orderBy,        // ORDER BY Price DESC
    10,             // LIMIT 10
    "Items",
    "dbo"
).ToList();

// Get 10 most expensive items
```

### Example 6: Update Operation

```csharp
var item = new Item { Id = 1, Name = "Laptop", Price = 1299.99m };

// Update using where columns (WHERE Id = 1)
client.Update(
    item,
    null,                  // update all columns
    new string[] { "Id" }, // WHERE condition
    "Items",
    "dbo"
);
client.CommitTransaction();
```

### Example 7: Batch Update

```csharp
var items = new List<Item>
{
    new Item { Id = 1, Name = "Item 1", Price = 15.00m },
    new Item { Id = 2, Name = "Item 2", Price = 25.00m }
};

client.UpdateBatch(
    items,
    null,                  // update all columns
    new string[] { "Id" }, // WHERE Id = item.Id
    "Items",
    "dbo"
);
client.CommitTransaction();
```

### Example 8: Delete Operation

```csharp
var item = new Item { Id = 1, Name = "Laptop", Price = 999.99m };

client.Delete(
    item,
    new string[] { "Id" },  // WHERE Id = 1
    "Items",
    "dbo"
);
client.CommitTransaction();
```

### Example 9: Working with Blob Data

```csharp
var imageData = System.IO.File.ReadAllBytes("photo.png");

var itemBlob = new ItemBlob 
{ 
    Id = 1, 
    BlobData = imageData 
};

client.Insert(itemBlob, null, "ItemBlobs", "dbo");
client.CommitTransaction();

// Retrieve blob
var result = client.Select<ItemBlob>(
    typeof(ItemBlob),
    null,
    new Dictionary<string, string> { { "Id", "1" } }
).SingleOrDefault();

System.IO.File.WriteAllBytes("photo_retrieved.png", result.BlobData);
```

### Example 10: Multi-Schema Operations (SQL Server)

```csharp
// Insert into Sales schema
var item = new Item { Id = 1, Name = "Widget", Price = 9.99m };
client.Insert(item, null, "Items", "Sales");

// Insert into Inventory schema
var inventory = new Inventory { ItemId = 1, Quantity = 100 };
client.Insert(inventory, null, null, "Inventory");

client.CommitTransaction();
```

---

## UPSERT Operations

UPSERT (INSERT OR UPDATE) is the most efficient way to handle insert-if-new, update-if-exists patterns.

### Insert or Update Pattern

```csharp
client.InsertOrUpdate<Item>(
    item,                                // object to insert or update
    insertColumns: new[] { "Id", "Name", "Price" },  // columns if inserting
    updateColumns: new[] { "Name", "Price" },        // columns if updating
    whereColumns: new[] { "Id" },                     // WHERE condition
    tableName: "Items",
    schemaName: "dbo"
);
client.CommitTransaction();
```

### How UPSERT Works Internally

**For SQL Server:**
```sql
IF EXISTS(SELECT 1 FROM Items WHERE Id = @Id)
BEGIN
    UPDATE Items SET Name = @Name, Price = @Price WHERE Id = @Id
END
ELSE
BEGIN
    INSERT INTO Items (Id, Name, Price) VALUES (@Id, @Name, @Price)
END
```

**For SQLite:**
```sql
UPDATE Items SET Name = @Name, Price = @Price WHERE EXISTS(SELECT 1 FROM Items WHERE Id = @Id);
INSERT INTO Items (Id, Name, Price) 
SELECT @Id, @Name, @Price 
WHERE NOT EXISTS(SELECT 1 FROM Items WHERE Id = @Id);
```

**For PostgreSQL:**
```sql
INSERT INTO Items (Id, Name, Price) VALUES (@Id, @Name, @Price)
ON CONFLICT(Id) DO UPDATE SET Name = @Name, Price = @Price;
```

### Use Cases for UPSERT

1. **Data Synchronization** - Sync external data into your database
2. **Bulk Data Import** - Import records, updating existing ones
3. **Cache Invalidation** - Update or create cache entries
4. **API Data Updates** - Handle duplicate API calls safely

```csharp
// Example: Synchronize product catalog
var products = FetchProductsFromExternalAPI();

foreach (var product in products)
{
    client.InsertOrUpdate<Product>(
        product,
        insertColumns: new[] { "Id", "Name", "Price", "Sku" },
        updateColumns: new[] { "Price", "Name" },  // Only update price and name
        whereColumns: new[] { "Sku" },              // Match by SKU
        "Products"
    );
}

client.CommitTransaction();
```

---

## Transaction Management

### Automatic Transactions

All FIK.ORM operations are wrapped in automatic transactions:

```csharp
// Automatic transaction - commits on success, rolls back on error
client.Insert(item, null, "Items");
client.CommitTransaction();
```

### Multiple Operations in Single Transaction

```csharp
try
{
    // All these operations are in the same transaction
    client.Insert(item1, null, "Items", "dbo");
    client.Insert(item2, null, "Items", "dbo");
    client.Update(item3, null, new[] { "Id" }, "Items", "dbo");

    // Commit all at once
    client.CommitTransaction();
}
catch (Exception ex)
{
    // Automatic rollback on any error
    Console.WriteLine($"Error: {ex.Message}");
    // Transaction is automatically rolled back
}
```

### Error Handling

```csharp
try
{
    var item = new Item { Id = 1, Name = "Item", Price = 9.99m };
    client.Insert(item, null, "Items");
    client.CommitTransaction();
}
catch (Exception ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
    // Transaction automatically rolled back
    // Exception message includes the generated SQL query for debugging
}
```

---

## Best Practices

### 1. Always Commit Transactions

```csharp
// ✅ Good
client.Insert(item, null, "Items", "dbo");
client.CommitTransaction();

// ❌ Avoid - forgetting to commit
client.Insert(item, null, "Items", "dbo");
```

### 2. Use Specific Where Columns for Updates/Deletes

```csharp
// ✅ Good - specify the key columns
client.Update(item, null, new string[] { "Id" }, "Items");

// ❌ Avoid - might update wrong records
client.Update(item, null, new string[] { "Name" }, "Items");
```

### 3. Filter Specific Columns When Possible

```csharp
// ✅ Good - only select needed columns
client.Insert(
    item,
    new[] { "Id", "Name", "Price" },  // Only these columns
    "Items"
);

// ✅ Also good - insert all columns
client.Insert(item, null, "Items");
```

### 4. Use Batch Operations for Multiple Records

```csharp
// ✅ Good - batch operation
client.InsertBatch(items, null, "Items");
client.CommitTransaction();

// ❌ Inefficient - loop with individual inserts
foreach (var item in items)
{
    client.Insert(item, null, "Items");
    client.CommitTransaction();
}
```

### 5. Use UPSERT for Insert-or-Update Patterns

```csharp
// ✅ Good - one atomic operation
client.InsertOrUpdate<Item>(
    item,
    new[] { "Id", "Name", "Price" },
    new[] { "Name", "Price" },
    new[] { "Id" },
    "Items"
);

// ❌ Inefficient - two operations, race condition possible
try
{
    client.Insert(item, null, "Items");
}
catch
{
    client.Update(item, null, new[] { "Id" }, "Items");
}
```

### 6. Handle Errors Gracefully

```csharp
try
{
    client.Insert(item, null, "Items");
    client.CommitTransaction();
}
catch (Exception ex)
{
    // Log the error - SQL query is included in exception message
    Logger.Error($"Failed to insert item: {ex.Message}");
    // Transaction is automatically rolled back
    throw;
}
```

### 7. Use Appropriate Schema Names

```csharp
// SQL Server - specify schema
client.Insert(item, null, "Items", "dbo");
client.Insert(item, null, "Items", "Sales");

// SQLite - schema name is ignored
client.Insert(item, null, "Items", "");

// PostgreSQL - specify schema
client.Insert(item, null, "Items", "public");
```

### 8. Dispose the Client When Done (if needed)

```csharp
using (var client = new QueryExecutorClient(
    connectionString, 
    DatabaseProvider.SqlServer))
{
    client.Insert(item, null, "Items");
    client.CommitTransaction();
}
// Resources automatically cleaned up
```

---

## Database-Specific Notes

### SQL Server

- **Schemas**: Use meaningful schema names like `dbo`, `Sales`, `Inventory`
- **Data Types**: Supports all SQL Server data types including `image`, `varbinary(max)`
- **Transaction Isolation**: Defaults to `ReadCommitted`
- **Transactions**: Automatic with rollback on error

### SQLite

- **Schemas**: Schema parameter is ignored; use empty string or any value
- **Concurrency**: Single-writer, multiple-reader
- **Transaction Behavior**: Automatically manages locking and isolation
- **Performance Tip**: Use batch operations for bulk inserts to optimize performance

### PostgreSQL

- **Schemas**: Use schema names like `public` or custom schemas
- **Data Types**: Supports BYTEA for binary data, JSON, JSONB, etc.
- **Transaction Isolation**: Configurable; defaults to `ReadCommitted`
- **Connection Pooling**: Consider using connection pooling for performance

---

## Troubleshooting

### "Database is locked" Error (SQLite)

**Cause**: Concurrent write access or incorrect transaction handling

**Solution**: 
- Ensure transactions are properly committed
- Use batch operations instead of individual inserts in loops
- Increase SQLite WAL (Write-Ahead Log) timeout

```csharp
// Enable WAL mode (improves concurrency)
var connectionString = "Data Source=mydb.db;Version=3;Journal Mode=Wal;";
```

### "Column not found" Error

**Cause**: Property names don't match database column names

**Solution**:
- Verify class property names match database column names (case-insensitive matching)
- Check for typos in table or column names
- Ensure schema name is correct

### "Type mismatch" Error

**Cause**: Property type doesn't match database column type

**Solution**:
- Use `decimal` for `MONEY`/`NUMERIC` columns
- Use `byte[]` for binary data
- Use `DateTime` for date/time columns

---

## Examples Repository

Comprehensive examples are available in the test files:

- `SQLQueryExecutorClientTest.cs` - SQL Server examples
- `SqLiteQueryExecutorClientTest.cs` - SQLite examples
- `PostgreSQLQueryExecutorClientTest.cs` - PostgreSQL examples

---

## Contributing

Contributions are welcome! Please ensure:

- Tests pass for all supported database providers
- Code follows existing style conventions
- New features include test coverage

---

## License

MIT License

---

## Support

For issues, questions, or suggestions:

- Create an issue on GitHub
- Check existing test files for usage examples
- Review the inline XML documentation

---

## Version History

### 2.0.0
- Initial release
- Support for SQL Server, SQLite, PostgreSQL
- New library structure with QueryExecutorClient
- Support for .NET Framework 4.0, .NET Standard 2.0, .NET 6.0, .NET 8.0
- CRUD operations with transaction support
- Batch operations
- UPSERT functionality

- 
### 1.0.0
- Initial release
- Support for SQL Server
- Different library structure without QueryExecutorClient
- Batch operations
- UPSERT functionality
