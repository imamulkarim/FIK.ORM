# FIK.ORM Project Structure Refactoring - Implementation Guide

## Quick Summary

**Status**: Analysis Complete ?
**Issues Found**: 4 minor issues (all fixable)
**Time to Fix**: ~30 minutes
**Complexity**: Low - mostly moving files and updating namespaces
**Breaking Changes**: None (internal organization only)

---

## ?? What Needs to Be Done

### Issue 1: Misnamed Factory Class ??
```
Current: DBObjectFactory
Issue: Uses DBin all caps (non-standard C# convention)
Fix: Rename to DbObjectFactory
```

### Issue 2-4: Misplaced Root Classes ??
```
Current Location:     Should Be:              Reason:
CompositeModel.cs  ?  Models/CompositeModel.cs     (Domain model)
OperationMode.cs   ?  Enums/OperationMode.cs       (Configuration enum)
DatabaseProvider.cs ? Enums/DatabaseProvider.cs    (Configuration enum)
```

---

## ?? Step-by-Step Implementation

### Step 1: Create Folders (2 minutes)

Create two new folders in your FIK.ORM project:

```
FIK.ORM/
??? Models/          (Create this)
??? Enums/           (Create this)
```

### Step 2: Rename File (1 minute)

In `FIK.ORM/Infrastructures/Data/`:
- Rename: `DBObjectFactory.cs` ? `DbObjectFactory.cs`
- Update: Class name from `DBObjectFactory` to `DbObjectFactory`

### Step 3: Move Files (5 minutes)

Move these files:
```
FIK.ORM/CompositeModel.cs          ? FIK.ORM/Models/CompositeModel.cs
FIK.ORM/OperationMode.cs           ? FIK.ORM/Enums/OperationMode.cs
FIK.ORM/DatabaseProvider.cs        ? FIK.ORM/Enums/DatabaseProvider.cs
```

### Step 4: Update Namespaces (10 minutes)

Update namespace in each moved file:

**Models/CompositeModel.cs**:
```csharp
// Change from: namespace FIK.ORM
namespace FIK.ORM.Models
{
    public class CompositeModel { }
}
```

**Enums/OperationMode.cs**:
```csharp
// Change from: namespace FIK.ORM
namespace FIK.ORM.Enums
{
    public enum OperationMode { }
}
```

**Enums/DatabaseProvider.cs**:
```csharp
// Change from: namespace FIK.ORM
namespace FIK.ORM.Enums
{
    public enum DatabaseProvider { }
}
```

### Step 5: Update All References (10 minutes)

Files that need updates - look for these usings:
```csharp
using FIK.ORM;  // These become:
using FIK.ORM.Models;  // For CompositeModel
using FIK.ORM.Enums;   // For OperationMode, DatabaseProvider
```

**Files to check**:
- `QueryExecutorClient.cs` - Most likely needs updates
- Any test files you've created
- Any consumer projects

### Step 6: Verify Build (5 minutes)

```bash
dotnet clean
dotnet build
# Should show: Build successful (0 errors, 0 warnings)
# All 4 frameworks should pass
```

---

## ?? Directory Structure After Changes

```
FIK.ORM/
?
??? QueryExecutorClient.cs          (Public API entry point)
?
??? Models/                         [NEW FOLDER]
?   ??? CompositeModel.cs          (Multi-table transaction builder)
?
??? Enums/                          [NEW FOLDER]
?   ??? OperationMode.cs           (Insert, Update, Delete, etc.)
?   ??? DatabaseProvider.cs        (SqlServer, SQLite, PostgreSQL)
?
??? Infrastructures/
?   ??? Data/
?   ?   ??? DbObjectFactory.cs     (Renamed from DBObjectFactory)
?   ??? MetaData/
?   ?   ??? IMetaDataProvider.cs
?   ?   ??? MetaDataProvider.cs
?   ?   ??? MetaDataProviderSQLite.cs
?   ?   ??? MetaDataProviderPostgreSQL.cs
?   ?   ??? MetaDataInfo.cs
?   ?   ??? MetaDataValidator.cs
?   ?   ??? MetaDataProviderFactory.cs
?   ??? QueryBuilders/
?   ?   ??? IQueryBuilder.cs
?   ?   ??? QueryBuilder.cs
?   ?   ??? QueryBuilderSQLite.cs
?   ?   ??? QueryBuilderFactory.cs
?   ??? Transactions/
?       ??? ITransactionManager.cs
?       ??? TransactionManager.cs
?       ??? ITransactionScope.cs
?       ??? TransactionScope.cs
?
??? Extensions/
    ??? [Extension methods]
```

---

## ?? Files Needing Reference Updates

### Primary Files:
1. **QueryExecutorClient.cs** - Check imports at top
2. **CompositeModel.cs** (before move) - Update namespace
3. **OperationMode.cs** (before move) - Update namespace
4. **DatabaseProvider.cs** (before move) - Update namespace

### Secondary Files:
- Any test files created
- Any example/sample files
- Documentation that references these classes

### Verification:
Use "Find All References" in your IDE:
- Search for `CompositeModel` ? update usings
- Search for `OperationMode` ? update usings
- Search for `DatabaseProvider` ? update usings
- Search for `DBObjectFactory` ? update name references

---

## ? Verification Checklist

After making all changes, verify:

- [ ] Both new folders created (`Models/`, `Enums/`)
- [ ] All 3 files moved to correct locations
- [ ] `DBObjectFactory` renamed to `DbObjectFactory`
- [ ] All 4 namespace declarations updated
- [ ] All `using` statements updated in consuming files
- [ ] Build successful on all 4 frameworks:
  - [ ] .NET Framework 4.0
  - [ ] .NET Standard 2.0
  - [ ] .NET 6.0
  - [ ] .NET 8.0
- [ ] Zero errors and zero warnings

---

## ?? What NOT to Change

Don't change:
- ? Class logic or implementation
- ? Method signatures
- ? Public API (except namespace)
- ? Infrastructure folder structure
- ? Other infrastructure naming

---

## ?? Benefits of This Refactoring

**After these changes**:
- ? Cleaner project organization
- ? Domain models properly grouped
- ? Configuration enums properly grouped
- ? Easier for new developers to find things
- ? Follows .NET naming conventions
- ? Better prepared for test project creation
- ? Aligns with professional standards

---

## ?? Next Steps After This Is Done

Once this refactoring is complete, you'll be ready to:
1. Create test projects for all frameworks
2. Write unit tests for all classes
3. Set up CI/CD pipeline
4. Prepare for release

---

## ?? Questions?

If you have questions about any specific file:
1. Check `PROJECT_STRUCTURE_ANALYSIS.md` for detailed explanation
2. Review the "Before/After" section for each change
3. Look at namespace organization diagram

---

**Estimated Time**: 30 minutes  
**Difficulty**: Easy  
**Priority**: High (do before test projects)  
**Breaking Changes**: None (internal only)

