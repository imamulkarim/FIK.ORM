# Git Workflow & Documentation Guidelines

## Overview
This document outlines the Git workflow, branching strategy, commit conventions, and documentation standards for the FIK.DAL modernization project.

---

## Branch Strategy

### Main Branches

#### 1. `master` - Production Release Branch
```
- Latest stable release
- All tests passing
- Code review approved
- Version tags on releases (v2.0.0, v2.1.0, etc.)
- Protected branch - no direct commits
- CI/CD pipeline active
```

**Protection Rules:**
- ? Require pull request reviews (minimum 2)
- ? Require status checks to pass
- ? Require code review approval
- ? Require up-to-date branches before merging
- ? Dismiss stale reviews when commits pushed
- ? Allow force pushes: NO

#### 2. `develop` - Development Branch
```
- Integration branch for features
- All tests must pass
- Staging area before master
- May contain pre-release versions
- Code review required but single approval sufficient
- CI pipeline active
```

### Feature Branches

#### Naming Convention
```
feature/{issue-number}-{description}
feature/123-transaction-management
feature/456-query-builder-optimization

bugfix/{issue-number}-{description}
bugfix/789-null-reference-fix
bugfix/234-connection-leak

hotfix/{version}-{description}
hotfix/2.0.1-critical-security-patch

refactor/{issue-number}-{description}
refactor/567-code-cleanup

docs/{description}
docs/update-readme-with-examples
```

#### Workflow
```
1. Create branch from: develop
2. Work on feature
3. Push regularly to backup
4. Create Pull Request to develop
5. Code review & CI checks pass
6. Merge to develop
7. Delete feature branch (automatic)
```

---

## Commit Convention

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type
```
feat:        New feature
fix:         Bug fix
docs:        Documentation only
style:       Changes that don't affect code logic (formatting, etc.)
refactor:    Code refactoring without feature changes
perf:        Performance improvements
test:        Test additions or modifications
chore:       Build process, dependencies, tooling
ci:          CI/CD configuration changes
revert:      Revert a previous commit
```

### Scope
```
transactions     - Transaction management
querybuilder     - Query building logic
metadata         - Metadata provider/validator
connection       - Connection management
executor         - Query executor client
core             - Core functionality
all              - Multiple areas
```

### Subject Guidelines
- ? Use imperative mood ("add" not "added" or "adds")
- ? Don't capitalize first letter
- ? No period (.) at the end
- ? Limit to 50 characters
- ? Reference issue number if applicable

### Body Guidelines
- ? Wrap at 72 characters
- ? Explain WHAT and WHY, not HOW
- ? Separate from subject with blank line
- ? Use bullet points for multiple items

### Footer Guidelines
```
Fixes #123
Closes #456
Relates to #789
Breaks: API change in Insert method
BREAKING CHANGE: Error handling pattern changed from ref string to exceptions
```

---

## Commit Examples

### Good Commit 1: Feature
```
feat(transactions): implement automatic transaction rollback

- Add ITransactionScope interface with Commit/Rollback methods
- Implement TransactionScope class with IDisposable pattern
- Add auto-rollback on exception in ExecuteInTransaction method
- Support savepoints for partial rollback (database dependent)

Closes #123
```

### Good Commit 2: Bug Fix
```
fix(metadata): remove orphaned method body from SQLite provider

The RetriveTableMetaData method had an orphaned code block containing
SQL Server specific queries. This was causing compilation errors in
the SQLite provider implementation.

Removed the unreachable code block that was querying sys.* tables.

Fixes #456
```

### Good Commit 3: Refactoring
```
refactor(executor): replace ref string ErrorMsg with exceptions

Changed error handling pattern throughout QueryExecutorClient:
- Remove ref string ErrorMsg parameters
- Throw specific exception types instead
- Update all method signatures
- Update calling code in tests

BREAKING CHANGE: Public API now throws exceptions instead of
returning bool with ref error parameter

Relates to #789
```

---

## Pull Request Process

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Breaking change
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Refactoring

## Related Issues
Closes #123

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests passed
- [ ] Tested on .NET 4.0
- [ ] Tested on .NET Standard 2.0
- [ ] Tested on .NET 6.0
- [ ] Tested on .NET 8.0

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Added tests for new functionality
- [ ] All tests passing locally

## Screenshots (if applicable)
[Add screenshots if UI related]

## Breaking Changes
Describe any breaking changes

## Migration Guide
Steps for users to migrate their code
```

### PR Code Review Checklist

**Reviewer Responsibility:**
- ? Code quality and standards
- ? Test coverage (min 80%)
- ? Documentation completeness
- ? Performance implications
- ? Security concerns
- ? API design review
- ? Cross-framework compatibility

**Code Review Process:**
```
1. Read PR description and related issues
2. Review test cases first (understand expected behavior)
3. Review code changes (logic, style, best practices)
4. Check documentation and comments
5. Request changes OR approve
6. Verify CI/CD passes before merge
```

---

## Release Process

### Version Numbering: SemVer

```
MAJOR.MINOR.PATCH
2      .0     .0

MAJOR - Breaking API changes
MINOR - New features, backward compatible
PATCH - Bug fixes, backward compatible

Examples:
v2.0.0  - Major modernization release
v2.0.1  - Bug fix release
v2.1.0  - New feature release
v3.0.0  - Next major version
```

### Release Branch Workflow

```
1. Create release branch from develop
   git checkout -b release/2.0.0 develop

2. Update version numbers
   - Update .csproj version
   - Update README.md
   - Update CHANGELOG.md

3. Create PR to master
   - Title: "Release v2.0.0"
   - Document release notes
   - Link to all related PRs

4. Merge to master
   - All tests pass
   - At least 2 approvals
   - Delete release branch

5. Tag release on master
   git tag -a v2.0.0 -m "Release version 2.0.0"
   git push origin v2.0.0

6. Create GitHub Release
   - Tag: v2.0.0
   - Title: Release v2.0.0
   - Body: Detailed changelog
   - Attach release notes

7. Merge back to develop
   git checkout develop
   git merge --no-ff master
   git push origin develop
```

### CHANGELOG.md Format

```markdown
# Changelog

All notable changes to FIK.DAL are documented in this file.

## [2.0.0] - 2024-01-15

### Added
- Transaction management infrastructure with ITransactionScope
- Automatic rollback on exception
- Async support for .NET 6.0+
- Savepoint support for partial rollbacks
- New QueryExecutorClient API

### Changed
- Error handling: replaced ref string ErrorMsg with exceptions
- All SQL/SQLite/PostgreSQL operations unified under QueryExecutorClient
- Improved resource management with IDisposable pattern

### Fixed
- Orphaned method body in SQLite metadata provider
- Null reference checks on transaction objects
- Connection leak issues

### Breaking Changes
- Insert/Update/Delete method signatures changed
- Error handling pattern changed (exceptions instead of bool + ref string)
- Parameter types changed (string columns ? IEnumerable<string>)

### Migration Guide
See MODERNIZATION_SUMMARY.md for detailed migration instructions

## [1.9.9] - 2023-12-01

### Added
- Initial SQLite support
- PostgreSQL support

### Fixed
- Various bug fixes
```

---

## Tag Management

### Creating Tags

```bash
# Annotated tag (recommended for releases)
git tag -a v2.0.0 -m "Release version 2.0.0 - Transaction modernization"

# List all tags
git tag -l

# Show tag details
git show v2.0.0

# Push tags to remote
git push origin v2.0.0          # Single tag
git push origin --tags          # All tags
git push origin v2.0.* --tags   # Pattern match
```

### Deleting Tags

```bash
# Delete local tag
git tag -d v2.0.0

# Delete remote tag
git push origin --delete v2.0.0
git push origin :refs/tags/v2.0.0  # Alternative
```

---

## Documentation Standards

### File Structure

```
/docs
??? README.md                           # Project overview
??? MODERNIZATION_SUMMARY.md           # This modernization overview
??? TESTING_GUIDE.md                   # Testing procedures
??? GIT_WORKFLOW.md                    # This file
??? API_MIGRATION.md                   # API migration guide
??? TRANSACTION_MANAGEMENT.md          # Transaction API docs
??? INSTALLATION.md                    # Installation guide
??? EXAMPLES.md                        # Code examples
??? TROUBLESHOOTING.md                 # Common issues
```

### Documentation Checklist

When submitting PR, ensure:
- ? Code comments for complex logic
- ? XML documentation comments on public APIs
- ? README updated if behavior changed
- ? CHANGELOG updated with entry
- ? Examples provided for new features
- ? Breaking changes documented
- ? Migration guide if needed

### XML Documentation Example

```csharp
/// <summary>
/// Executes a callback function within a transaction scope,
/// automatically committing on success or rolling back on exception.
/// </summary>
/// <typeparam name="TResult">The return type of the operation.</typeparam>
/// <param name="operation">The operation to execute within the transaction scope.</param>
/// <returns>The result of the operation.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="operation"/> is null.
/// </exception>
/// <exception cref="InvalidOperationException">
/// Thrown when a transaction is already active.
/// </exception>
/// <example>
/// <code>
/// var result = transactionManager.ExecuteInTransaction(scope =>
/// {
///     // Your database operations here
///     return 42;
/// });
/// </code>
/// </example>
public TResult ExecuteInTransaction<TResult>(Func<ITransactionScope, TResult> operation)
{
    // Implementation
}
```

---

## CI/CD Integration

### GitHub Actions Configuration

```yaml
name: Build and Test

on:
  push:
    branches: [ master, develop ]
  pull_request:
    branches: [ master, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: [ '4.0', 'standard2.0', '6.0', '8.0' ]

    steps:
    - uses: actions/checkout@v2

    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: ${{ matrix.dotnet-version }}

    - name: Restore
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release

    - name: Test
      run: dotnet test --configuration Release --no-build

    - name: Code Coverage
      run: dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

    - name: Upload Coverage
      uses: codecov/codecov-action@v2
      with:
        file: ./coverage.opencover.xml
```

---

## Common Git Commands

### Working with Feature Branches

```bash
# Create feature branch
git checkout -b feature/123-new-feature develop

# Work on feature
git add .
git commit -m "feat(scope): description"

# Keep up with develop
git fetch origin
git rebase origin/develop

# Push to remote
git push origin feature/123-new-feature

# Create Pull Request (via GitHub)

# After merge, clean up
git checkout develop
git pull origin develop
git branch -d feature/123-new-feature
```

### Fixing Commits

```bash
# Amend last commit
git commit --amend

# Amend with new changes
git add .
git commit --amend

# Force push (only on feature branches!)
git push origin feature/123-new-feature --force-with-lease

# Interactive rebase last 3 commits
git rebase -i HEAD~3

# Squash commits
git rebase -i develop
# Mark commits with 's' to squash
```

### Code Review

```bash
# Switch to PR branch
git checkout origin/feature-branch

# Test locally
dotnet build
dotnet test

# Add review comment
git review

# Push review
git review --publish
```

---

## Best Practices

### DO ?
- ? Create small, focused PRs (< 400 lines of code)
- ? Write clear commit messages
- ? Test locally before pushing
- ? Keep branches up to date with develop
- ? Document significant changes
- ? Request reviews from domain experts
- ? Update documentation with code changes
- ? Use squash merge for feature branches

### DON'T ?
- ? Commit directly to master or develop
- ? Write vague commit messages ("fix stuff")
- ? Force push to shared branches
- ? Mix unrelated changes in one PR
- ? Leave TODO comments without context
- ? Merge without passing tests
- ? Forget to update CHANGELOG
- ? Create massive PRs (> 1000 lines)

---

## Troubleshooting

### Accidentally Committed to Master

```bash
# Create new branch from master
git branch feature/accidentally-committed

# Reset master to previous state
git reset --hard HEAD~1

# Push changes
git push origin feature/accidentally-committed

# Create PR to develop
```

### Need to Revert a Merge

```bash
# Revert entire PR
git revert -m 1 <merge-commit-sha>

# Push revert
git push origin develop
```

### Fix Commit Message After Push

```bash
# Amend and force push (feature branches only!)
git commit --amend -m "new message"
git push origin feature-branch --force-with-lease
```

### Merge Conflicts

```bash
# List conflicts
git status

# Manually resolve conflicts in editor

# Mark as resolved
git add conflicted-file.cs

# Continue rebase
git rebase --continue

# Or abort
git rebase --abort
```

---

## References

- [Git Documentation](https://git-scm.com/doc)
- [GitHub Flow Guide](https://guides.github.com/introduction/flow/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)

---

## Contact & Questions

For questions about Git workflow:
1. Review this documentation
2. Check GitHub discussions
3. Open an issue
4. Contact maintainers

