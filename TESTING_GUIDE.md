# TruePal.Api - Testing Quick Reference

## 🎯 The Golden Rule

> **When you add or modify a feature, you MUST update tests.**  
> **No exceptions. No excuses. Tests are non-negotiable.**

---

## ⚡ Quick Start

### Before You Start Coding
```bash
# Navigate to test project
cd TruePal.Api.Tests

# Run existing tests to ensure clean baseline
dotnet test

# ✅ All tests should pass
```

### After You Code
```bash
# Create/update tests in appropriate folder
# - Repositories/ for data layer
# - Services/ for business logic
# - Integration/ for end-to-end tests

# Run tests again
dotnet test

# ✅ All tests must pass before committing
```

---

## 📝 Test Template

Copy this template for new tests:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TruePal.Api.Data;
using TruePal.Api.Infrastructure.Repositories;
using TruePal.Api.Models;

namespace TruePal.Api.Tests.Repositories;

public class YourFeatureRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly YourFeatureRepository _repository;

    public YourFeatureRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new YourFeatureRepository(_context);
    }

    [Fact]
    public async Task MethodName_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange - Set up test data
        var entity = new YourEntity
        {
            Property = "value"
        };

        // Act - Execute the method
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Assert - Verify expected outcome
        entity.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MethodName_ShouldFail_WhenInvalidDataProvided()
    {
        // Arrange
        var invalidEntity = new YourEntity { /* invalid data */ };

        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

## ✅ Minimum Test Requirements

### For Each CRUD Operation:

**CREATE (Add new entity)**
- ✅ Success case with valid data
- ✅ ID auto-generation test
- ✅ Validation failure test (if applicable)

**READ (Get entity)**
- ✅ Get by ID when exists
- ✅ Get by ID when not exists (returns null)
- ✅ Get all entities
- ✅ Filter/search tests (if applicable)

**UPDATE (Modify entity)**
- ✅ Update with valid changes
- ✅ Preserve non-updated fields
- ✅ Validation tests

**DELETE (Remove entity)**
- ✅ Delete existing entity
- ✅ Verify entity removed
- ✅ Other entities not affected

---

## 📐 Test Naming Convention

**Format:** `MethodName_ExpectedBehavior_StateUnderTest`

### ✅ Good Examples:
```csharp
CreateUser_ShouldSucceed_WhenValidDataProvided
GetUserByEmail_ShouldReturnNull_WhenEmailNotFound
UpdateProfile_ShouldPreserveCreatedAt_WhenUpdating
DeleteUser_ShouldRemoveUser_WhenUserExists
ValidatePassword_ShouldFail_WhenPasswordTooShort
```

### ❌ Bad Examples:
```csharp
TestCreateUser()      // Not descriptive
Test1()               // Meaningless
UserTest()            // Vague
CreateUserTest()      // Doesn't specify outcome
```

---

## 🧪 Arrange-Act-Assert Pattern

**Every test should follow this structure:**

```csharp
[Fact]
public async Task ExampleTest()
{
    // ARRANGE - Set up test data and dependencies
    var user = new User
    {
        Username = "testuser",
        Email = "test@example.com"
    };
    await _repository.AddAsync(user);
    await _context.SaveChangesAsync();

    // ACT - Execute the method being tested
    var result = await _repository.GetByEmailAsync("test@example.com");

    // ASSERT - Verify the expected outcome  
    result.Should().NotBeNull();
    result!.Username.Should().Be("testuser");
}
```

---

## 💎 FluentAssertions Cheat Sheet

### Common Assertions:

```csharp
// Null checks
result.Should().NotBeNull();
result.Should().BeNull();

// Equality
result.Should().Be(expectedValue);
result.Should().NotBe(unexpectedValue);

// Strings
result.Title.Should().Be("Expected Title");
result.Name.Should().Contain("substring");
result.Email.Should().StartWith("test");

// Numbers
result.Count.Should().BeGreaterThan(0);
result.Id.Should().BeLessThan(100);
result.Age.Should().BeInRange(18, 65);

// Booleans
result.IsActive.Should().BeTrue();
result.IsDeleted.Should().BeFalse();

// Collections
result.Should().HaveCount(3);
result.Should().Contain(item);
result.Should().NotContain(unwantedItem);
result.Should().ContainSingle(x => x.Id == 1);

// Types
result.Should().BeOfType<User>();
result.Should().BeAssignableTo<IUser>();

// Result<T> Pattern
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();
result.Errors.Should().Contain("Expected error message");
```

---

## 🚫 Common Mistakes to Avoid

### ❌ DON'T: Share database context between tests
```csharp
// BAD - Tests will interfere with each other
public class BadTests
{
    private static AppDbContext _context; // ❌ Static shared context
}
```

### ✅ DO: Create fresh context for each test class
```csharp
// GOOD - Isolated tests
public class GoodTests : IDisposable
{
    private readonly AppDbContext _context; // ✅ Instance per test class
    
    public GoodTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ✅ Unique DB
            .Options;
        _context = new AppDbContext(options);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted(); // ✅ Clean up
        _context.Dispose();
    }
}
```

### ❌ DON'T: Test multiple things in one test
```csharp
[Fact]
public async Task TestEverything() // ❌ Too broad
{
    await CreateUser();
    await UpdateUser();
    await DeleteUser();
}
```

### ✅ DO: One test per behavior
```csharp
[Fact]
public async Task CreateUser_ShouldSucceed() { } // ✅ Focused

[Fact]
public async Task UpdateUser_ShouldSucceed() { } // ✅ Focused

[Fact]
public async Task DeleteUser_ShouldSucceed() { } // ✅ Focused
```

---

## 🎯 Pre-Commit Checklist

Before committing code:

```bash
# 1. Build succeeds
dotnet build
# ✅ No errors

# 2. All tests pass
cd TruePal.Api.Tests
dotnet test
# ✅ All green

# 3. New tests added for new features
# ✅ Verify new test files created

# 4. Test coverage adequate
# ✅ Minimum requirements met (see main standards)
```

**If ANY test fails → DO NOT COMMIT**

---

## 📊 Test Organization

```
TruePal.Api.Tests/
├── Repositories/           # Data layer tests
│   ├── UserRepositoryTests.cs
│   └── PostRepositoryTests.cs
├── Services/              # Business logic tests
│   ├── AuthServiceTests.cs
│   └── PostServiceTests.cs
├── Integration/           # End-to-end tests
│   ├── UserIntegrationTests.cs
│   └── PostIntegrationTests.cs
└── README.md             # Test documentation
```

**File Naming:** `{ClassBeingTested}Tests.cs`

---

## 🔍 Running Specific Tests

```bash
# Run all tests
dotnet test

# Run tests in specific file
dotnet test --filter "FullyQualifiedName~UserRepositoryTests"

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateUser_ShouldSucceed"

# Run with verbose output
dotnet test --verbosity detailed

# Run tests matching pattern
dotnet test --filter "DisplayName~Create"
```

---

## 💡 Pro Tips

1. **Write tests first** (TDD) - Helps clarify requirements
2. **Test edge cases** - Empty strings, null values, boundary conditions
3. **Use descriptive test names** - Should read like documentation
4. **Keep tests simple** - One assertion concept per test
5. **Don't test framework code** - Focus on YOUR logic
6. **Mock external dependencies** - Database, APIs, file system
7. **Run tests frequently** - Catch issues early

---

## 🚨 Remember

> **Code without tests is broken by design.**  
> **Tests are not optional. They are required.**  
> **No tests = No merge.**

---

## 📚 References

- Full Testing Rules: [CODING_STANDARDS.md - Rules 27-34](CODING_STANDARDS.md#testing-requirements)
- Test Examples: [TruePal.Api.Tests/README.md](TruePal.Api.Tests/README.md)
- xUnit Documentation: https://xunit.net/
- FluentAssertions Documentation: https://fluentassertions.com/

---

**Questions?** Check [CODING_STANDARDS.md](CODING_STANDARDS.md) or ask the team.

**Last Updated:** April 13, 2026
