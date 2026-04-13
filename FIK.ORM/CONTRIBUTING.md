# Contributing to FIK.ORM

## Code Style Guidelines

- Follow Microsoft's C# Coding Conventions
- Use meaningful variable and method names
- Include XML documentation for public APIs
- Keep methods focused and concise

## Building and Packaging

### Local Development Build

```bash
dotnet build -c Debug
```

### Release Build (All Frameworks)

```bash
dotnet build -c Release
```

### Generating NuGet Package

For local testing:
```bash
dotnet pack -c Release -o ./artifacts
```


# Clean previous builds
dotnet clean

# Restore dependencies
dotnet restore

# Build all frameworks
dotnet build -c Release

# Create package
dotnet pack -c Release -o ./artifacts


This generates a single `.nupkg` file containing binaries for all 4 target frameworks:
- .NET Framework 4.0 (`net40`)
- .NET Standard 2.0 (`netstandard2.0`)
- .NET 6.0 (`net6.0`)
- .NET 8.0 (`net8.0`)

### Publishing to NuGet.org

```bash
dotnet nuget push ./artifacts/FIK.ORM.*.nupkg -s https://api.nuget.org/v3/index.json -k YOUR_API_KEY
```

**Note**: Always increment the version in `FIK.ORM.csproj` before publishing:

```xml
<Version>2.0.1</Version>  <!-- Update this -->
```

## Testing

Run all tests:
```bash
dotnet test -c Release
```

Test specific framework:
```bash
dotnet test -f net6.0 -c Release
```

## Pull Request Process

1. Ensure all tests pass across all frameworks
2. Update version number if making public changes
3. Update CHANGELOG if applicable
4. Include test cases for new features
5. Ensure XML documentation is complete

## Database Support

- **SQL Server**: Supported via ADO.NET
- **SQLite**: Requires `System.Data.SQLite` (included in package)
- **PostgreSQL**: Requires `Npgsql` (included for .NET 6.0+)

## Key Points for Package Distribution

- ✅ GeneratePackageOnBuild is enabled
- ✅ All frameworks build to the same .nupkg
- ✅ Framework-specific dependencies are automatically resolved
- ✅ XML documentation included for IntelliSense
- ✅ LICENSE file included in package