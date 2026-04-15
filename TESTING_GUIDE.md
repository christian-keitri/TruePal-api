# TruePal.Api - Testing Guide

## The Golden Rule

> **When you add or modify a feature, you MUST update tests.**
> **No tests = No merge.**

---

## Quick Start

```bash
# Run all tests
cd TruePal.Api.Tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~PostServiceTests"

# Run specific test
dotnet test --filter "FullyQualifiedName~CreatePostAsync_ReturnsSuccess"

# Verbose output
dotnet test --verbosity detailed
```

---

## Test Stack

| Tool | Purpose |
|------|---------|
| xUnit | Test framework |
| FluentAssertions | Readable assertion syntax |
| In-memory SQLite | Test database isolation |
| NullLogger | Suppress log output in tests |

---

## Test File Organization

Mirror the source code structure:

```
TruePal.Api.Tests/
├── Helpers/
│   └── TestDbContext.cs           # In-memory SQLite setup
├── Repositories/
│   ├── UserRepositoryTests.cs
│   └── PostRepositoryTests.cs
├── Services/
│   ├── AuthServiceTests.cs
│   └── PostServiceTests.cs
└── TruePal.Api.Tests.csproj
```

**File naming:** `{ClassBeingTested}Tests.cs`
**Class naming:** `public class {ClassBeingTested}Tests`
**Namespace:** matches folder path (`TruePal.Api.Tests.Repositories`, `TruePal.Api.Tests.Services`)

---

## Test Template

### Repository Tests

```csharp
using FluentAssertions;
using TruePal.Api.Infrastructure.Repositories;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests.Repositories;

public class YourEntityRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly YourEntityRepository _repository;

    public YourEntityRepositoryTests()
    {
        _testDb = new TestDbContext();
        _repository = new YourEntityRepository(_testDb.Context);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        var entity = new YourEntity { Name = "Test" };
        await _testDb.Context.YourEntities.AddAsync(entity);
        await _testDb.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    public void Dispose() => _testDb.Dispose();
}
```

### Service Tests

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TruePal.Api.Application.Services;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Infrastructure;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests.Services;

public class YourServiceTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly UnitOfWork _unitOfWork;
    private readonly YourService _service;

    public YourServiceTests()
    {
        _testDb = new TestDbContext();
        _unitOfWork = new UnitOfWork(_testDb.Context);
        _service = new YourService(_unitOfWork, NullLogger<YourService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        // ...

        // Act
        var result = await _service.CreateAsync(...);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _testDb.Dispose();
    }
}
```

---

## FluentAssertions Cheat Sheet

### Basics
```csharp
result.Should().NotBeNull();
result.Should().BeNull();
result.Should().Be(expectedValue);
result.Should().NotBe(unexpectedValue);
```

### Booleans
```csharp
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();
```

### Strings
```csharp
result.Content.Should().Be("Expected");
result.Error.Should().Contain("not found");
result.Email.Should().StartWith("test");
```

### Numbers
```csharp
result.Id.Should().BeGreaterThan(0);
result.Count.Should().Be(3);
result.ViewsCount.Should().BeInRange(1, 100);
```

### Collections
```csharp
result.Should().HaveCount(3);
result.Should().BeEmpty();
result.Should().OnlyContain(p => p.UserId == userId);
result.First().Content.Should().Be("Expected");
```

### Result Pattern
```csharp
result.IsSuccess.Should().BeTrue();
result.Data.Should().NotBeNull();
result.Data!.Content.Should().Be("Expected");

result.IsSuccess.Should().BeFalse();
result.Error.Should().Be("Post not found");
result.ErrorCode.Should().Be(ErrorCodes.NotFound);
result.Errors.Should().Contain("Content is required");
```

---

## Test Naming Convention

**Format:** `MethodName_ExpectedBehavior_StateUnderTest`

```csharp
// Good
CreatePostAsync_ReturnsSuccess_WhenValidInput()
GetPostByIdAsync_ReturnsNotFound_WhenPostDoesNotExist()
UpdatePostAsync_ReturnsForbidden_WhenWrongUser()
DeletePostAsync_RemovesPost_WhenOwnerDeletes()
IncrementViewsAsync_IncrementsCount_WhenPostExists()

// Bad
TestCreatePost()       // Not descriptive
Test1()                // Meaningless
PostTest()             // Vague
```

---

## Minimum Test Coverage

### Per Repository (CRUD)

| Operation | Minimum Tests |
|-----------|---------------|
| CREATE | Success case + ID auto-generated |
| READ | Exists + not exists + list all |
| UPDATE | Success + preserves unchanged fields |
| DELETE | Success + verify removed |

### Per Service

| Category | Minimum Tests |
|----------|---------------|
| Success path | At least 1 per method |
| Validation failure | Empty input, too long, invalid format |
| Authorization | Wrong user (FORBIDDEN) |
| Not found | Non-existent resource (NOT_FOUND) |
| Error codes | Verify correct ErrorCode on each failure |
| Edge cases | Null optionals, boundary values |

---

## Rules

1. **Use FluentAssertions** - not `Assert.Equal` / `Assert.True`
2. **One behavior per test** - don't test create + update + delete in one method
3. **Arrange-Act-Assert** - every test follows this structure
4. **Isolated databases** - each test class gets its own `TestDbContext`
5. **Test error codes** - verify `ErrorCode` matches, not just error messages
6. **No shared state** - tests must not depend on execution order
7. **No commented-out tests** - remove or fix, never comment out
8. **Run before committing** - `dotnet test` must pass, always

---

## Pre-Commit Checklist

```bash
# 1. Build
dotnet build
# Must succeed with 0 errors

# 2. Tests
cd TruePal.Api.Tests && dotnet test
# Must show: Test Run Successful

# 3. New tests exist for new features
# Verify in Repositories/ or Services/ directories

# If ANY test fails -> DO NOT COMMIT
```

---

**Last Updated:** April 15, 2026
