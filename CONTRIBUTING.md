# Contributing to TruePal.Api

## Required Reading

Before writing any code:
1. [CODING_STANDARDS.md](CODING_STANDARDS.md) - 57 mandatory rules
2. [ARCHITECTURE.md](ARCHITECTURE.md) - System design and patterns
3. [TESTING_GUIDE.md](TESTING_GUIDE.md) - Test requirements

---

## Development Workflow

### 1. Setup

```bash
dotnet restore
dotnet ef database update
dotnet run
```

### 2. Create Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Write Code Following Standards

Key patterns to follow:
- Access database through `IUnitOfWork` (never DbContext directly)
- Services return `Result<T>` with `ErrorCodes` (never throw for business logic)
- MVC controllers inherit `BaseController`; API controllers use `ControllerBase` + `[ApiController]`
- Request DTOs have `[Required]`, `[StringLength]` annotations
- Response DTOs have `FromModel()` factory methods (never expose domain models)
- Try-catch in controllers, not services
- Controllers use `MapError()` to route `ErrorCode` to HTTP status

### 4. Write Tests

Every feature must have tests. No exceptions.

```bash
cd TruePal.Api.Tests
dotnet test  # verify baseline passes first
```

Then add your tests:
- Repository tests in `Repositories/`
- Service tests in `Services/`
- Use FluentAssertions (`.Should().Be(...)`)
- Use Arrange-Act-Assert pattern
- Test error codes, not just error messages

### 5. Verify Before Committing

```bash
# Build
dotnet build
# 0 errors required

# Tests
cd TruePal.Api.Tests && dotnet test
# All tests must pass

# Theme Testing (if UI changes)
dotnet run
# Open http://localhost:5087
# Click theme toggle (moon/sun icon)
# Verify your changes work in BOTH dark and light modes
```

### 6. Commit

```bash
git add .
git commit -m "feat: descriptive message"
```

Prefixes: `feat:` (new feature), `fix:` (bug fix), `refactor:`, `test:`, `docs:`

---

## Pull Request Checklist

### Code
- [ ] Follows [CODING_STANDARDS.md](CODING_STANDARDS.md)
- [ ] Uses existing patterns (Result, UnitOfWork, DTOs)
- [ ] No direct DbContext access (uses UnitOfWork)
- [ ] Services return `Result<T>` with `ErrorCodes`
- [ ] Controllers have try-catch and use `MapError()`
- [ ] Response DTOs used (no anonymous objects or domain models in responses)

### Validation
- [ ] DTOs have data annotations
- [ ] Service validates business rules
- [ ] All validation errors returned at once

### Security
- [ ] No passwords in plain text
- [ ] User input escaped where rendered
- [ ] Ownership checked before mutations
- [ ] `[Authorize]` on protected endpoints
- [ ] `[ValidateAntiForgeryToken]` on MVC POST actions

### UI/UX & Theming
- [ ] All colors use CSS variables (no hardcoded colors)
- [ ] Component tested in BOTH dark and light modes
- [ ] Text is readable in both themes
- [ ] No white-on-white or black-on-black issues
- [ ] Follows [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md)
- [ ] Follows [THEME_GUIDE.md](THEME_GUIDE.md)

### Tests
- [ ] Tests added for new/modified features
- [ ] FluentAssertions used (not `Assert.X`)
- [ ] Tests in correct subdirectory (`Repositories/` or `Services/`)
- [ ] Error codes verified in failure tests
- [ ] All tests pass (`dotnet test`)

### Build
- [ ] `dotnet build` succeeds with 0 errors
- [ ] No new compiler warnings in changed files

---

## Common Mistakes

### Don't bypass the service layer
```csharp
// BAD - controller talks to repository
var post = await _unitOfWork.Posts.GetByIdAsync(id);

// GOOD - controller talks to service
var result = await _postService.GetPostByIdAsync(id);
```

### Don't match errors by string
```csharp
// BAD
if (result.Error.Contains("not found")) return NotFound();

// GOOD
return result.ErrorCode switch {
    ErrorCodes.NotFound => NotFound(new { error = result.Error }),
    ...
};
```

### Don't use anonymous objects in responses
```csharp
// BAD - duplicated everywhere, drifts
return Ok(new { id = post.Id, content = post.Content, ... });

// GOOD - single source of truth
return Ok(PostResponse.FromPost(post));
```

### Don't skip error codes
```csharp
// BAD - controller can't distinguish 404 from 403
return Result<Post>.Failure("Post not found");

// GOOD
return Result<Post>.Failure("Post not found", ErrorCodes.NotFound);
```

---

## Code Review Process

PRs are reviewed for:
1. **Standards compliance** (blocking)
2. **Security** (blocking)
3. **Test coverage** (blocking)
4. **Architecture** (blocking)
5. **Performance** (advisory)

PRs that violate coding standards or lack tests are rejected.

---

**Last Updated:** April 15, 2026
