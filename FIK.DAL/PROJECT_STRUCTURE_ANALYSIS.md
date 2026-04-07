# FIK.ORM Project Structure Review & Recommendations

## ?? Current Project Overview

**Project**: FIK.ORM.csproj (Multi-target)
**Framework Targets**: .NET Framework 4.0, .NET Standard 2.0, .NET 6.0, .NET 8.0
**Version**: 2.0.0
**Package ID**: FIK.ORM

---

## ??? Current Directory Structure Analysis

### Root Level Classes
```
FIK.ORM/
??? QueryExecutorClient.cs          ? (Main entry point - Good location)
??? CompositeModel.cs               ?? (Should be in Models folder)
??? OperationMode.cs (enum)         ?? (Should be in Enums folder)
??? DatabaseProvider.cs (enum)      ?? (Should be in Enums folder)

Infrastructures/
??? Data/
?   ??? DBObjectFactory.cs          ? (Good organization)
??? MetaData/
?   ??? IMetaDataProvider.cs        ? (Good organization)
?   ??? MetaDataInfo.cs             ? (Good organization)
?   ??? MetaDataProvider.cs
?   ??? MetaDataProviderSQLite.cs
?   ??? MetaDataProviderPostgreSQL.cs
?   ??? MetaDataValidator.cs        ? (Good organization)
??? QueryBuilders/
?   ??? IQueryBuilder.cs            ? (Good organization)
?   ??? QueryBuilder.cs
?   ??? QueryBuilderFactory.cs      ? (Good organization)
?   ??? QueryBuilderSQLite.cs
??? Transactions/
    ??? ITransactionManager.cs      ? (Good organization)
    ??? TransactionManager.cs       ? (Good organization)
    ??? ITransactionScope.cs        ? (Good organization)
    ??? TransactionScope.cs         ? (Good organization)

Extensions/                         ? (Exists - good)
??? [Extension methods]
```

---

## ?? Identified Issues & Recommendations

### Issue 1: Misplaced Root-Level Classes (CRITICAL)

**Problem**:
```
CompositeModel.cs              ? In root - should be in Models
OperationMode.cs (enum)        ? In root - should be in Enums
DatabaseProvider.cs (enum)     ? In root - should be in Enums
```

**Why it matters**:
- Clutters root namespace
- Violates organization best practices
- Inconsistent with Infrastructure folder structure
- Hard to find domain models

**Recommendation**: Create `Models` and `Enums` folders

---

### Issue 2: Naming Convention Inconsistencies

**Current Issues**:

| Class/File | Current Name | Issue | Recommended Name |
|------------|--------------|-------|------------------|
| CompositeModel | CompositeModel | ? Good name | Keep as is |
| Enum | OperationMode | ? Good name | Keep as is |
| Enum | DatabaseProvider | ? Good name | Keep as is |
| Factory | DBObjectFactory | ?? DB abbreviation | DbObjectFactory |
| Provider | MetaDataProvider | ? Good name | Keep as is |
| SQLite Provider | MetaDataProviderSQLite | ? Good name | Keep as is |
| PostgreSQL Provider | MetaDataProviderPostgreSQL | ? Good name | Keep as is |
| Interface | IMetaDataProvider | ? Good name | Keep as is |
| Validator | MetaDataValidator | ? Good name | Keep as is |
| Transactions | ITransactionScope | ? Good name | Keep as is |
| Transactions | TransactionScope | ? Good name | Keep as is |

**Minor Issues**:
- `DBObjectFactory` ? `DbObjectFactory` (C# naming: DB not DBV)
- Most other names follow proper conventions ?

---

### Issue 3: Factory Classes Organization

**Current State**:
```
Data/
??? DBObjectFactory.cs            ? Good

QueryBuilders/
??? QueryBuilderFactory.cs        ? Good

MetaData/
??? [No factory visible]          ?? But MetaDataProviderFactory likely exists
```

**Recommendation**: Consider creating `Factories/` folder to consolidate:
```
Infrastructures/
??? Factories/
?   ??? DbObjectFactory.cs        (moved from Data/)
?   ??? QueryBuilderFactory.cs    (moved from QueryBuilders/)
?   ??? MetaDataProviderFactory.cs (if exists)
??? Data/
??? MetaData/
??? QueryBuilders/
??? Transactions/
```

---

### Issue 4: Namespace Organization Analysis

**Current Namespaces** ?:
```
? FIK.ORM                          (Root - public API)
? FIK.ORM.Infrastructures           (Internal infrastructure)
? FIK.ORM.Infrastructures.Data      (Data access)
? FIK.ORM.Infrastructures.MetaData  (Metadata)
? FIK.ORM.Infrastructures.QueryBuilders
? FIK.ORM.Infrastructures.Transactions
? FIK.ORM.Extensions               (Extension methods)
```

**Issues**:
- Namespaces are well-organized ?
- Consistent with folder structure ?
- Internal classes properly scoped ?

---

## ?? Recommended Directory Structure

### Option A: Minimal Changes (Recommended for Now)

```
FIK.ORM/
??? Models/                         [NEW FOLDER]
?   ??? CompositeModel.cs          (moved)
?   ??? OperationMode.cs           (moved - enum)
?
??? Constants/                      [OPTIONAL]
?   ??? DatabaseProvider.cs        (moved - enum)
?
??? QueryExecutorClient.cs         (keep - public API entry point)
?
??? Infrastructures/               (keep as is)
?   ??? Data/
?   ?   ??? DbObjectFactory.cs     (renamed from DBObjectFactory)
?   ??? MetaData/
?   ??? QueryBuilders/
?   ??? Transactions/
?   ??? Factories/                 [OPTIONAL - consolidate factories]
?
??? Extensions/                    (keep as is)
```

### Option B: Comprehensive Refactoring (Future Enhancement)

```
FIK.ORM/
??? Public API/
?   ??? QueryExecutorClient.cs
?   ??? IRepository.cs
?   ??? IQueryable.cs
?
??? Models/
?   ??? CompositeModel.cs
?   ??? MetaDataInfo.cs
?   ??? QueryResult.cs
?
??? Enums/
?   ??? OperationMode.cs
?   ??? DatabaseProvider.cs
?   ??? IsolationLevel.cs
?   ??? SqlProvider.cs
?
??? Exceptions/
?   ??? DataAccessException.cs
?   ??? QueryExecutionException.cs
?
??? Infrastructure/
?   ??? Data/
?   ??? MetaData/
?   ??? QueryBuilders/
?   ??? Transactions/
?   ??? Factories/
?   ??? Mappers/
?
??? Extensions/
    ??? [Extension methods]
```

---

## ? Recommended Action Plan

### Step 1: Minor Naming Fix (5 minutes)
**Rename DBObjectFactory ? DbObjectFactory**

```csharp
// File: FIK.ORM/Infrastructures/Data/DbObjectFactory.cs
public static class DbObjectFactory  // Changed from DBObjectFactory
{
    // Implementation remains same
}
```

**Affected Files to Update**:
- File renamed: `DBObjectFactory.cs` ? `DbObjectFactory.cs`
- Usages updated in: `QueryExecutorClient.cs` (if used directly)

### Step 2: Organize Root Classes into Folders (15 minutes)

**Create Folders**:
```
mkdir FIK.ORM/Models
mkdir FIK.ORM/Enums
```

**Move Files**:
```
Move CompositeModel.cs ? FIK.ORM/Models/CompositeModel.cs
Move OperationMode.cs ? FIK.ORM/Enums/OperationMode.cs
Move DatabaseProvider.cs ? FIK.ORM/Enums/DatabaseProvider.cs
```

**Update Namespaces in Moved Files**:
```csharp
// CompositeModel.cs
namespace FIK.ORM.Models
{
    public class CompositeModel { }
}

// OperationMode.cs
namespace FIK.ORM.Enums
{
    public enum OperationMode { }
}

// DatabaseProvider.cs
namespace FIK.ORM.Enums
{
    public enum DatabaseProvider { }
}
```

**Update Usages** (in all files that reference these):
```csharp
// Before
using FIK.ORM;
var composite = new CompositeModel();

// After
using FIK.ORM.Models;
using FIK.ORM.Enums;
var composite = new CompositeModel();
var provider = DatabaseProvider.SqlServer;
```

### Step 3: Update Project File (5 minutes)

**Update FIK.ORM.csproj** - Verify Include patterns are correct:
```xml
<ItemGroup>
    <Compile Include="Models/**/*.cs" />
    <Compile Include="Enums/**/*.cs" />
    <Compile Include="Infrastructures/**/*.cs" />
    <Compile Include="Extensions/**/*.cs" />
</ItemGroup>
```

### Step 4: Verify Build (5 minutes)
```bash
dotnet clean
dotnet build
# Ensure 0 errors, 0 warnings on all frameworks
```

---

## ?? Files to Update After Reorganization

### Files That Reference These Classes:

**QueryExecutorClient.cs** - Check imports:
```csharp
// Update to:
using FIK.ORM.Models;
using FIK.ORM.Enums;
```

**Any Test Files** - Update references:
```csharp
using FIK.ORM.Models;
using FIK.ORM.Enums;
```

**Any External Projects** - If these classes are public API:
```csharp
// Update package references in consumer projects
using FIK.ORM.Models;
using FIK.ORM.Enums;
```

---

## ?? Class Naming Analysis - No Major Issues ?

### Public Classes - Naming Conventions ?

| Class | Type | Naming | Status | Notes |
|-------|------|--------|--------|-------|
| QueryExecutorClient | Public | PascalCase | ? | Perfect |
| CompositeModel | Public | PascalCase | ? | Good |
| OperationMode | Enum | PascalCase | ? | Good |
| DatabaseProvider | Enum | PascalCase | ? | Good |
| MetaDataProvider | Public | PascalCase | ? | Good |
| TransactionManager | Public | PascalCase | ? | Good |
| TransactionScope | Public | PascalCase | ? | Good |

### Internal Classes - Naming Conventions ?

| Class | Type | Naming | Status | Notes |
|-------|------|--------|--------|-------|
| QueryBuilder | Internal | PascalCase | ? | Good |
| MetaDataValidator | Internal | PascalCase | ? | Good |
| QueryBuilderFactory | Internal | PascalCase | ? | Good |
| DbObjectFactory | Internal | PascalCase | ? | Good |

### One Minor Naming Issue ??

| Class | Current | Issue | Recommended |
|-------|---------|-------|-------------|
| DBObjectFactory | DBObject... | DB (all caps) | DbObjectFactory |

**Why**: C# naming convention uses `Db` not `DB`

---

## ?? Implementation Priority

### Phase 1: Immediate (Before Test Project Creation) - 30 minutes
- [ ] Rename `DBObjectFactory` ? `DbObjectFactory`
- [ ] Create `Models` folder
- [ ] Create `Enums` folder  
- [ ] Move `CompositeModel.cs` ? `Models/`
- [ ] Move `OperationMode.cs` ? `Enums/`
- [ ] Move `DatabaseProvider.cs` ? `Enums/`
- [ ] Update namespaces in moved files
- [ ] Update usages throughout codebase
- [ ] Run build to verify (0 errors)

### Phase 2: Enhancement (Optional - Future) - 1-2 hours
- [ ] Create `Factories/` folder for all factories
- [ ] Create `Exceptions/` folder for custom exceptions
- [ ] Organize by concern (Optional but recommended)

### Phase 3: Advanced (Future Enhancement) - 2-4 hours
- [ ] Add proper folder structure for all concerns
- [ ] Separate interfaces from implementations
- [ ] Organize utilities and helpers

---

## ? Summary of Findings

### Positive Findings ?
```
? Namespace organization is excellent
? Infrastructure separation is well-done
? Factory pattern properly implemented
? Transaction management properly isolated
? Metadata handling well-organized
? Query builder separation is clean
? Class naming conventions followed (mostly)
? Interface segregation principle applied
```

### Minor Issues Found ??
```
?? DBObjectFactory should be DbObjectFactory (1 class)
?? Root-level classes should be in folders (3 classes):
   - CompositeModel ? Models/
   - OperationMode ? Enums/
   - DatabaseProvider ? Enums/
```

### No Critical Issues ?
```
? No critical naming issues
? No namespace pollution
? No circular dependencies detected
? No organization conflicts
```

---

## ?? Before You Create Test Projects

### Action Items (Recommended - 30 minutes)

1. **Rename One File**:
   ```
   DBObjectFactory.cs ? DbObjectFactory.cs
   ```
   Update class name and usages.

2. **Move Three Files**:
   ```
   CompositeModel.cs ? Models/CompositeModel.cs
   OperationMode.cs ? Enums/OperationMode.cs
   DatabaseProvider.cs ? Enums/DatabaseProvider.cs
   ```
   Update namespaces in each file.

3. **Update References**:
   - Find usages of moved files
   - Update `using` statements
   - Verify build succeeds

4. **Verify Build**:
   ```bash
   dotnet clean
   dotnet build
   # All frameworks should pass
   ```

### Why Do This Before Testing?

- ? Cleaner structure for test project references
- ? Better organization for future team members
- ? Proper namespace hierarchy established
- ? No technical debt carried forward
- ? Aligns with .NET best practices

---

## ?? Quick Before/After

### Before:
```
FIK.ORM/
??? QueryExecutorClient.cs
??? CompositeModel.cs          ?? Misplaced
??? OperationMode.cs           ?? Misplaced
??? DatabaseProvider.cs        ?? Misplaced
??? Infrastructures/...
??? Extensions/...
```

### After:
```
FIK.ORM/
??? QueryExecutorClient.cs
??? Models/
?   ??? CompositeModel.cs      ? Organized
??? Enums/
?   ??? OperationMode.cs       ? Organized
?   ??? DatabaseProvider.cs    ? Organized
??? Infrastructures/...
??? Extensions/...
```

---

## ?? Code Changes Required (if implementing)

### File 1: Rename and Move `DBObjectFactory.cs`

**Location**: `FIK.ORM/Infrastructures/Data/DBObjectFactory.cs` 
? `FIK.ORM/Infrastructures/Data/DbObjectFactory.cs`

```csharp
// Rename class from DBObjectFactory to DbObjectFactory
public static class DbObjectFactory  // Changed from DBObjectFactory
{
    // Rest remains same
}
```

### File 2: Move `CompositeModel.cs`

**Location**: `FIK.ORM/CompositeModel.cs` 
? `FIK.ORM/Models/CompositeModel.cs`

```csharp
// Update namespace
namespace FIK.ORM.Models  // Changed from FIK.ORM
{
    public class CompositeModel
    {
        // Implementation remains same
    }
}
```

### File 3: Move `OperationMode.cs`

**Location**: `FIK.ORM/OperationMode.cs` 
? `FIK.ORM/Enums/OperationMode.cs`

```csharp
// Update namespace
namespace FIK.ORM.Enums  // Changed from FIK.ORM
{
    public enum OperationMode
    {
        // Values remain same
    }
}
```

### File 4: Move `DatabaseProvider.cs`

**Location**: `FIK.ORM/DatabaseProvider.cs` 
? `FIK.ORM/Enums/DatabaseProvider.cs`

```csharp
// Update namespace
namespace FIK.ORM.Enums  // Changed from FIK.ORM
{
    public enum DatabaseProvider
    {
        // Values remain same
    }
}
```

### File 5: Update `QueryExecutorClient.cs` (and any other files)

```csharp
// Update usings:
using FIK.ORM.Models;
using FIK.ORM.Enums;

// Usage remains same:
var composite = new CompositeModel();
var provider = DatabaseProvider.SqlServer;
```

---

## ? Final Recommendation

**Implement Phase 1 (30 minutes) before creating test projects**:
- Rename `DBObjectFactory` ? `DbObjectFactory` (1 file)
- Move 3 files to `Models/` and `Enums/` folders
- Update 4-5 namespace declarations
- Update usages in 5-10 files
- Verify build (0 errors on all frameworks)

**Result**: 
- ? Clean project structure
- ? Better organized codebase
- ? Ready for test projects
- ? Follows .NET best practices
- ? Easier for new team members

---

**Recommendation Status**: ?? **IMPLEMENT BEFORE TEST PROJECT CREATION**

