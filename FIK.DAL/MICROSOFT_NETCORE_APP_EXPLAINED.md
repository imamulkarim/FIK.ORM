# Microsoft.NETCore.App - Usage & Explanation in FIK.DAL

## What is Microsoft.NETCore.App?

`Microsoft.NETCore.App` is a **shared framework package** that contains the .NET Core runtime and base class libraries. It's **automatically referenced** when you target .NET Core/NET 5+ projects.

---

## Is It Explicitly Used in Your Project?

### ? **NO** - Not Explicitly Referenced

Looking at your project files:

#### FIK.DAL.Core.csproj (netcoreapp2.1)
```xml
<TargetFramework>netcoreapp2.1</TargetFramework>
<PackageReference Include="System.Data.Common" Version="4.3.0" />
<PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
```
? **No explicit reference** to `Microsoft.NETCore.App`

#### FIK.DAL.PostgreSQL.csproj (net5.0)
```xml
<TargetFramework>net5.0</TargetFramework>
<PackageReference Include="Npgsql" Version="6.0.2" />
```
? **No explicit reference** to `Microsoft.NETCore.App`

#### FIK.DAL.SQLite.csproj (netstandard2.0)
```xml
<TargetFramework>netstandard2.0</TargetFramework>
```
? **No reference needed** (targets .NET Standard, not Core-specific)

#### FIK.DAL.csproj (.NET Framework 4.0)
```xml
<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
```
? **Not applicable** (uses .NET Framework, not .NET Core)

---

## Why Doesn't Your Project Explicitly Reference It?

### 1. **Implicit/Automatic Reference**
When you specify `<TargetFramework>netcoreapp2.1</TargetFramework>` or `<TargetFramework>net5.0</TargetFramework>`:
- The build system **automatically includes** `Microsoft.NETCore.App` as a framework reference
- You don't need to (and shouldn't) add it manually
- It provides access to core types like `System.Object`, `System.String`, `System.Collections`, etc.

### 2. **Framework Reference vs Package Reference**
```xml
<!-- ? DON'T DO THIS (explicit) -->
<PackageReference Include="Microsoft.NETCore.App" Version="2.1.0" />

<!-- ? DO THIS (implicit via TargetFramework) -->
<TargetFramework>netcoreapp2.1</TargetFramework>
```

### 3. **.NET Framework 4.0 Doesn't Use It**
- `.NET Framework 4.0` uses the full Framework runtime
- It ships with the complete BCL (Base Class Library)
- No separate core package needed

### 4. **.NET Standard 2.0 is Framework-Agnostic**
- `netstandard2.0` can run on either .NET Core or .NET Framework
- It references the `NETStandard.Library` package instead
- Consumer projects provide the actual runtime

---

## How Microsoft.NETCore.App Works

### When You Target .NET Core

```
Your Project (netcoreapp2.1)
        ?
    (implicit reference)
        ?
Microsoft.NETCore.App (framework package)
        ?
Contains:
  ??? System.Runtime
  ??? System.Collections
  ??? System.Linq
  ??? System.Reflection
  ??? System.Data.SqlClient (in some versions)
  ??? System.ComponentModel (you use this!)
  ??? ... many more
```

---

## What Your Projects Actually Reference

### FIK.DAL.Core (netcoreapp2.1)
| Package | Source | Purpose |
|---------|--------|---------|
| `Microsoft.NETCore.App` | **Implicit** (Framework) | Core runtime & BCL |
| `System.Data.Common` | **Explicit** (PackageReference) | IDbConnection, IDbCommand interfaces |
| `System.Data.SqlClient` | **Explicit** (PackageReference) | SqlConnection, SqlCommand classes |

### FIK.DAL.PostgreSQL (net5.0)
| Package | Source | Purpose |
|---------|--------|---------|
| `Microsoft.NETCore.App` | **Implicit** (Framework) | Core runtime & BCL |
| `Npgsql` | **Explicit** (PackageReference) | PostgreSQL database provider |

### FIK.DAL.SQLite (netstandard2.0)
| Package | Source | Purpose |
|---------|--------|---------|
| `NETStandard.Library` | **Implicit** (Framework) | .NET Standard BCL |
| `System.Data.SQLite` | **Manual Reference** (HintPath) | SQLite database provider |

### FIK.DAL (.NET Framework 4.0)
| Package | Source | Purpose |
|---------|--------|---------|
| Framework (implicit) | **Built-in** | Complete .NET Framework BCL |

---

## Classes in Your Code Using Microsoft.NETCore.App Types

### FIK.DAL.Core/SQL.cs Example

```csharp
using System;                               // From Microsoft.NETCore.App
using System.Collections.Generic;           // From Microsoft.NETCore.App
using System.ComponentModel;                // From Microsoft.NETCore.App
using System.Data;                          // From Microsoft.NETCore.App
using System.Data.SqlClient;                // From System.Data.SqlClient package
using System.Reflection;                    // From Microsoft.NETCore.App

public class SQL
{
    private SqlConnection connection;        // Explicit package reference

    public SQL(string connectionString)
    {
        connection = new SqlConnection(connectionString);
    }

    // Uses PropertyDescriptor from System.ComponentModel
    // which comes from Microsoft.NETCore.App
    PropertyDescriptorCollection props = 
        TypeDescriptor.GetProperties(typeof(T));
}
```

**Types from Microsoft.NETCore.App:**
- ? `System` (Exception, String, etc.)
- ? `System.Collections.Generic` (List<T>, Dictionary<K,V>)
- ? `System.ComponentModel` (PropertyDescriptor, TypeDescriptor)
- ? `System.Data` (IDbConnection, DbNull, etc.)
- ? `System.Reflection` (PropertyInfo, BindingFlags)
- ? `System.Text` (StringBuilder)
- ? `System.Linq` (LINQ methods)

**Types from explicit packages:**
- ? `System.Data.SqlClient` (SqlConnection, SqlCommand)
- ? `Npgsql` (NpgsqlConnection, NpgsqlCommand)
- ? `System.Data.SQLite` (SQLiteConnection, SQLiteCommand)

---

## Should You Ever Explicitly Reference Microsoft.NETCore.App?

### ? **NO** - In normal circumstances
```xml
<!-- Don't add this manually -->
<PackageReference Include="Microsoft.NETCore.App" Version="2.1.0" />
```

### ? **YES ONLY IF:**

1. **Using RuntimeIdentifier in a self-contained app**
```xml
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
</PropertyGroup>
```

2. **Pinning specific runtime version** (advanced scenario)
```xml
<ItemGroup>
    <FrameworkReference Include="Microsoft.NETCore.App" Version="2.1.30" />
</ItemGroup>
```

---

## Your Project's Current Setup - Is It Correct?

### ? **YES - Your approach is correct!**

By letting `<TargetFramework>` implicitly reference `Microsoft.NETCore.App`:
- ? Build system manages dependencies automatically
- ? No version conflicts
- ? Works across different machines
- ? Follows .NET tooling best practices
- ? Cleaner, more maintainable project files

---

## Summary

| Aspect | Answer |
|--------|--------|
| **Is it used in FIK.DAL?** | ? Yes, implicitly by netcoreapp2.1 and net5.0 projects |
| **Is it explicitly referenced?** | ? No, and it shouldn't be |
| **Why not?** | Automatically included when targeting .NET Core/5+ |
| **Should you add it manually?** | ? No, unless using self-contained deployments |
| **Can you see it in project file?** | ? Not visible, but it's there implicitly |
| **Does it affect your code?** | ? Yes, provides core types like System, Collections, Reflection |

Your project follows the **correct modern .NET approach** by not explicitly referencing it! ??

