# TruePal.Api - Coding Standards

## Purpose

Mandatory coding patterns for TruePal.Api. All pull requests must comply. Non-compliance results in PR rejection.

---

## Table of Contents

1. [Architecture Rules](#architecture-rules)
2. [MVC Controller Standards](#mvc-controller-standards)
3. [API Controller Standards](#api-controller-standards)
4. [Service Layer Rules](#service-layer-rules)
5. [Repository Pattern Rules](#repository-pattern-rules)
6. [Error Handling Rules](#error-handling-rules)
7. [Validation Rules](#validation-rules)
8. [Scalability Rules](#scalability-rules)
9. [Reliability Rules](#reliability-rules)
10. [API Design & UX Rules](#api-design--ux-rules)
11. [Security Rules](#security-rules)
12. [CSS & Component Rules](#css--component-rules)
13. [Testing Requirements](#testing-requirements)
14. [Naming Conventions](#naming-conventions)

---

## Architecture Rules

### RULE 1: Follow Clean Architecture Layers

All code must reside in its designated layer. Never bypass layers.

```
Controllers/         -> Receives requests, delegates to services, returns responses
Application/Services -> Business logic, validation, orchestration
Core/Interfaces      -> Contracts (IPostService, IPostRepository, Result<T>)
Core/Validators      -> Reusable validation helpers
Infrastructure/      -> Database access (repositories), middleware
Models/              -> Domain entities
DTOs/                -> Request/response data transfer objects
```

**Never** let a controller access a repository directly. Always go through a service.

### RULE 2: All Dependencies Must Use Interfaces

```csharp
// BAD - concrete dependency
private readonly PostService _postService;

// GOOD - interface dependency
private readonly IPostService _postService;
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IPostService, PostService>();
```

### RULE 3: Use the Result Pattern for All Service Returns

Services never throw exceptions for business logic. They return `Result<T>`.

```csharp
public async Task<Result<Post>> CreatePostAsync(...)
{
    if (invalid)
        return Result<Post>.Failure("Reason", ErrorCodes.Validation);

    return Result<Post>.Success(post);
}
```

### RULE 4: Use Error Codes on All Failure Results

Every `Result.Failure()` call must include an `ErrorCode` so controllers can map to the correct HTTP status without string matching.

```csharp
// Defined in Core/Common/Result.cs
public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string Validation = "VALIDATION";
}

// Usage in services
return Result<Post>.Failure("Post not found", ErrorCodes.NotFound);
return Result<Post>.Failure("Not authorized", ErrorCodes.Forbidden);
return Result<Post>.Failure(validationErrors); // auto-tagged VALIDATION
```

### RULE 5: Access Database Only Through UnitOfWork

```csharp
// BAD
_context.Posts.Add(post);
await _context.SaveChangesAsync();

// GOOD
await _unitOfWork.Posts.AddAsync(post);
await _unitOfWork.CompleteAsync();
```

### RULE 6: Repositories Must Extend the Generic Base

```csharp
public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetByUserIdAsync(int userId);
}

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context) { }
}
```

This inherits `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Delete`, `FindAsync`, `AnyAsync`.

---

## MVC Controller Standards

### RULE 7: MVC Controllers Inherit from BaseController

```csharp
public class PostsController : BaseController
{
    public PostsController(IConfiguration configuration, ILogger<PostsController> logger)
        : base(logger, configuration) { }
}
```

This provides: `SetSuccess()`, `SetError()`, `SetInfo()`, `SetWarning()`, `RedirectToActionWithSuccess()`, `AddErrors()`, `LogAndDisplayError()`.

### RULE 8: Use Strongly-Typed ViewModels

```csharp
// BAD
ViewData["Username"] = user.Username;

// GOOD
var model = new ProfileViewModel { Username = user.Username };
return View(model);
```

Define ViewModels at the bottom of controller files in a `#region ViewModels` block.

### RULE 9: Use ValidateAntiForgeryToken on All MVC POST Actions

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreatePostViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    // ...
}
```

### RULE 10: Check Authentication Before Protected Resources

```csharp
if (!Request.Cookies.ContainsKey("AuthToken"))
{
    return RedirectToActionWithError("Login", "Auth", "Please login to continue");
}
```

### RULE 11: Try-Catch in MVC Controllers, Not Services

```csharp
// Controller
try
{
    var result = await _postService.CreatePostAsync(model);
    if (!result.IsSuccess)
    {
        AddErrors(result.Errors);
        return View(model);
    }
    return RedirectToActionWithSuccess("Index", "Posts", "Post created!");
}
catch (Exception ex)
{
    LogAndDisplayError("Failed to create post", ex);
    return View(model);
}

// Service - returns Result, no try-catch
public async Task<Result<Post>> CreatePostAsync(...)
{
    // business logic only
}
```

---

## API Controller Standards

### RULE 12: API Controllers Inherit from ControllerBase

API controllers use `[ApiController]` with `ControllerBase`, not `BaseController` (which is for MVC views).

```csharp
[ApiController]
[Route("api/posts")]
public class ApiPostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<ApiPostsController> _logger;

    public ApiPostsController(IPostService postService, ILogger<ApiPostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }
}
```

### RULE 13: Use MapError for Consistent Error Responses

Every API controller must include a `MapError` helper that routes `ErrorCode` to HTTP status:

```csharp
private IActionResult MapError(string? error, string? errorCode, List<string>? errors = null)
{
    if (errors != null && errors.Count > 0)
        return BadRequest(new { errors });

    return errorCode switch
    {
        ErrorCodes.NotFound => NotFound(new { error }),
        ErrorCodes.Forbidden => StatusCode(403, new { error }),
        ErrorCodes.Validation => BadRequest(new { error }),
        _ => BadRequest(new { error })
    };
}
```

**Never** use `Forbid()` in API controllers (returns empty body). Use `StatusCode(403, new { error })`.

### RULE 14: Try-Catch in API Controllers

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetPost(int id)
{
    try
    {
        var result = await _postService.GetPostByIdAsync(id);
        if (!result.IsSuccess)
            return MapError(result.Error!, result.ErrorCode);

        return Ok(PostResponse.FromPost(result.Data!));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving post {PostId}", id);
        return StatusCode(500, new { error = "An error occurred while retrieving the post" });
    }
}
```

### RULE 15: Extract Repeated Claim Parsing into Helpers

```csharp
private bool TryGetUserId(out int userId)
{
    userId = 0;
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    return userIdClaim != null && int.TryParse(userIdClaim.Value, out userId);
}
```

### RULE 16: Use Response DTOs, Never Expose Domain Models

```csharp
// BAD - leaks internal structure
return Ok(post);

// BAD - duplicated anonymous objects
return Ok(new { id = post.Id, content = post.Content, ... });

// GOOD - dedicated response DTO
return Ok(PostResponse.FromPost(post));
```

Response DTOs live in `DTOs/` with a static `FromModel()` factory method:

```csharp
public class PostResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    // ...

    public static PostResponse FromPost(Post post, bool includeTrendingScore = false)
    {
        return new PostResponse { Id = post.Id, Content = post.Content, ... };
    }
}
```

---

## Service Layer Rules

### RULE 17: Services Return Result, No Try-Catch

Services contain business logic only. They return `Result<T>` for success/failure. Exception handling belongs in controllers and middleware.

```csharp
public async Task<Result<Post>> CreatePostAsync(int userId, string content, ...)
{
    var errors = ValidatePostInput(content, location, imageUrl);
    if (errors.Count > 0)
        return Result<Post>.Failure(errors);

    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user == null)
        return Result<Post>.Failure("User not found", ErrorCodes.NotFound);

    var post = new Post { ... };
    await _unitOfWork.Posts.AddAsync(post);
    await _unitOfWork.CompleteAsync();

    return Result<Post>.Success(post);
}
```

### RULE 18: Validate All Inputs in Service Layer

Validate content length, field constraints, and business rules before touching the database. Return all errors at once, not one at a time.

```csharp
private static List<string> ValidatePostInput(string content, string? location, string? imageUrl)
{
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(content))
        errors.Add("Post content is required");
    else if (content.Length > 500)
        errors.Add("Post content must be 500 characters or less");
    if (location != null && location.Length > 200)
        errors.Add("Location must be 200 characters or less");
    if (imageUrl != null && imageUrl.Length > 500)
        errors.Add("Image URL must be 500 characters or less");
    return errors;
}
```

### RULE 19: Enforce Ownership Before Mutations

Any update or delete must verify the requesting user owns the resource:

```csharp
if (post.UserId != userId)
    return Result<Post>.Failure("You are not authorized to update this post", ErrorCodes.Forbidden);
```

---

## Repository Pattern Rules

### RULE 20: Always Include Navigation Properties When Needed

Use `.Include()` for related data. Never rely on lazy loading.

```csharp
public async Task<Post?> GetByIdWithUserAsync(int id)
{
    return await _dbSet
        .Include(p => p.User)
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

### RULE 21: Keep Computed Properties Aligned with Queries

If a model has a `[NotMapped]` computed property used in API responses, its formula must match any repository ordering that uses the same concept.

```csharp
// Model
[NotMapped]
public double TrendingScore =>
    (LikesCount * 2) + (CommentsCount * 3) + (ViewsCount * 0.5);

// Repository - MUST use same formula
.OrderByDescending(p => p.LikesCount * 2 + p.CommentsCount * 3 + p.ViewsCount * 0.5)
```

---

## Error Handling Rules

### RULE 22: Never Expose Raw Exception Messages

```csharp
// BAD
catch (Exception ex) { return BadRequest(new { error = ex.Message }); }

// GOOD
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating post");
    return StatusCode(500, new { error = "An error occurred while creating the post" });
}
```

### RULE 23: Never Route Errors by String Matching

```csharp
// BAD - fragile, breaks when message text changes
if (result.Error.Contains("not found"))
    return NotFound();

// GOOD - typed error codes
return result.ErrorCode switch
{
    ErrorCodes.NotFound => NotFound(new { error = result.Error }),
    ErrorCodes.Forbidden => StatusCode(403, new { error = result.Error }),
    _ => BadRequest(new { error = result.Error })
};
```

### RULE 24: Global Exception Middleware as Safety Net

`GlobalExceptionMiddleware` catches any unhandled exceptions. It is a safety net, not a substitute for proper error handling in controllers.

---

## Validation Rules

### RULE 25: Use Data Annotations on All DTOs

```csharp
public class CreatePostDto
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 500 characters")]
    public string Content { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Location must be 200 characters or less")]
    public string? Location { get; set; }
}
```

### RULE 26: Validate at Both DTO and Service Layers

- **DTO annotations** catch malformed requests before they reach the service (via `[ApiController]` auto-validation).
- **Service validation** enforces business rules (user exists, ownership, uniqueness).

Both layers are required. DTOs catch shape errors; services catch logic errors.

### RULE 27: Return All Validation Errors at Once

```csharp
// BAD - user fixes one error, submits, gets another
if (string.IsNullOrWhiteSpace(content))
    return Result<Post>.Failure("Content required");

// GOOD - user sees all problems at once
var errors = new List<string>();
if (string.IsNullOrWhiteSpace(content)) errors.Add("Content required");
if (content.Length > 500) errors.Add("Content too long");
return Result<Post>.Failure(errors);
```

---

## Scalability Rules

### RULE 28: Always Use Async/Await for I/O Operations

Every database call, HTTP request, or file operation must be async. Method names must end with `Async`.

```csharp
// BAD
public Post GetPostById(int id) => _context.Posts.Find(id);

// GOOD
public async Task<Post?> GetByIdAsync(int id) =>
    await _dbSet.FindAsync(id);
```

### RULE 29: Paginate All List Endpoints

Never return unbounded collections. All list endpoints must accept `count` (or `pageSize` + `page`) and enforce a maximum.

```csharp
// Service
if (count <= 0 || count > 100)
    return Result<IEnumerable<Post>>.Failure("Count must be between 1 and 100", ErrorCodes.Validation);

// Repository
return await _dbSet.OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();
```

### RULE 30: Prevent N+1 Queries

Always `.Include()` required navigation properties in the repository query. Never access navigation properties that weren't loaded.

```csharp
// BAD - N+1: loads posts then hits DB for each post's user
var posts = await _dbSet.ToListAsync();
foreach (var p in posts) Console.Write(p.User.Username); // N queries

// GOOD - single query with join
var posts = await _dbSet.Include(p => p.User).ToListAsync();
```

### RULE 31: Use Scoped Lifetime for Database Dependencies

Repositories, UnitOfWork, and DbContext must be registered as `Scoped` (one instance per request). Never use `Singleton` for database access.

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<AppDbContext>(...);
```

---

## Reliability Rules

### RULE 32: GET Endpoints Must Be Idempotent

GET requests must not modify state. Side effects (view counting, analytics) belong in separate POST endpoints.

```csharp
// BAD - GET modifies data
[HttpGet("{id}")]
public async Task<IActionResult> GetPost(int id)
{
    var post = await _postService.GetPostByIdAsync(id);
    await _postService.IncrementViewsAsync(id); // side effect in GET
    return Ok(post);
}

// GOOD - separate endpoint
[HttpGet("{id}")]
public async Task<IActionResult> GetPost(int id) { ... }

[HttpPost("{id}/views")]
public async Task<IActionResult> IncrementViews(int id) { ... }
```

### RULE 33: Use UnitOfWork for Atomic Operations

When a service method performs multiple writes, they all go through the same UnitOfWork and a single `CompleteAsync()` call. If any step fails, nothing is committed.

```csharp
await _unitOfWork.Posts.AddAsync(post);
await _unitOfWork.CompleteAsync(); // single commit
```

### RULE 34: Log All Service Operations

Use structured logging with named parameters for operations that change state:

```csharp
_logger.LogInformation("Post {PostId} created by user {UserId}", post.Id, userId);
_logger.LogError(ex, "Error creating post for user {UserId}", userId);
```

### RULE 35: Middleware Pipeline Order Matters

The middleware pipeline must follow this order:
1. `GlobalExceptionMiddleware` (catches everything)
2. `RequestLoggingMiddleware` (logs timing)
3. `UseHttpsRedirection()`
4. `UseStaticFiles()`
5. `UseAuthentication()`
6. `UseAuthorization()`

---

## API Design & UX Rules

### RULE 36: Consistent JSON Response Format

**Success (single):** `{ "id": 1, "content": "...", "user": { ... } }`
**Success (list):** `[{ "id": 1, ... }, { "id": 2, ... }]`
**Error (single):** `{ "error": "Post not found" }`
**Error (multiple):** `{ "errors": ["Content required", "Location too long"] }`
**Server error:** `{ "error": "An error occurred while ..." }`

### RULE 37: Use Correct HTTP Status Codes

| Status | When |
|--------|------|
| 200 OK | Successful GET, PUT |
| 201 Created | Successful POST (with `CreatedAtAction`) |
| 204 No Content | Successful DELETE |
| 400 Bad Request | Validation errors |
| 401 Unauthorized | Missing or invalid auth token |
| 403 Forbidden | Authenticated but not authorized (e.g., wrong owner) |
| 404 Not Found | Resource does not exist |
| 500 Internal Server Error | Unhandled exception |

### RULE 38: Use CreatedAtAction for POST Responses

```csharp
return CreatedAtAction(
    nameof(GetPost),
    new { id = result.Data!.Id },
    PostResponse.FromPost(result.Data));
```

This returns 201 with a `Location` header pointing to the new resource.

### RULE 39: Use camelCase for All JSON Properties

Response DTOs use PascalCase in C# but `System.Text.Json` auto-converts to camelCase. Never override this. API consumers expect `createdAt`, not `CreatedAt`.

### RULE 40: Null Fields Should Be Omitted or Explicit

Optional fields (like `trendingScore`) should be `null` when not applicable, not `0` or empty string. Use `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` or handle in the response DTO factory.

---

## Security Rules

### RULE 41: HttpOnly Secure Cookies for Auth Tokens

```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
};
Response.Cookies.Append("AuthToken", token, cookieOptions);
```

### RULE 42: Never Store Passwords in Plain Text

Always use BCrypt:
```csharp
string hash = BCrypt.Net.BCrypt.HashPassword(password);
bool valid = BCrypt.Net.BCrypt.Verify(password, hash);
```

### RULE 43: Escape User-Generated Content in JavaScript

```javascript
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
```

### RULE 44: JWT Tokens Must Validate All Claims

```csharp
TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    // ...
};
```

---

## CSS & Component Rules

### RULE 45: Follow Component-Based CSS Architecture

```
wwwroot/css/
├── theme.css           # Global theme variables (always loaded)
├── components/         # Reusable component styles
│   ├── cards.css
│   ├── panels.css
│   └── overlays.css
└── pages/              # Page-specific styles
    ├── home.css
    └── dashboard.css
```

### RULE 46: Use CSS Variables from Theme

```css
/* BAD */
.card { background: #c9a961; padding: 16px; }

/* GOOD */
.card { background: var(--primary-yellow); padding: var(--spacing-md); }
```

### RULE 47: Load CSS Per-Page

```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/components/cards.css" asp-append-version="true">
    <link rel="stylesheet" href="~/css/pages/posts.css" asp-append-version="true">
}
```

---

## Testing Requirements

### RULE 48: All Features Must Include Tests

No exceptions. PRs without tests are automatically rejected.

### RULE 49: Use FluentAssertions

```csharp
// BAD
Assert.NotNull(result);
Assert.Equal("test@example.com", result.Email);
Assert.True(result.IsActive);

// GOOD
result.Should().NotBeNull();
result!.Email.Should().Be("test@example.com");
result.IsActive.Should().BeTrue();
```

### RULE 50: Follow Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task CreatePostAsync_ValidInput_ReturnsSuccess()
{
    // Arrange
    var userId = _testUser.Id;
    var content = "Test post";

    // Act
    var result = await _postService.CreatePostAsync(userId, content, null, null);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data!.Content.Should().Be(content);
}
```

### RULE 51: Test Naming Convention

Format: `MethodName_ExpectedBehavior_StateUnderTest`

```
CreatePostAsync_ReturnsSuccess_WhenValidInput
GetPostByIdAsync_ReturnsNotFound_WhenPostDoesNotExist
DeletePostAsync_ReturnsForbidden_WhenWrongUser
```

### RULE 52: Test File Organization

Mirror the source structure:
```
TruePal.Api.Tests/
├── Helpers/
│   └── TestDbContext.cs
├── Repositories/
│   ├── UserRepositoryTests.cs
│   └── PostRepositoryTests.cs
└── Services/
    ├── AuthServiceTests.cs
    └── PostServiceTests.cs
```

### RULE 53: Minimum Test Coverage Per Feature

**Repository:** CREATE (2 tests), READ (3 tests), UPDATE (2 tests), DELETE (2 tests)
**Service:** Success path, validation failures, authorization checks, edge cases
**Each error code path** must have at least one test.

### RULE 54: Isolate Tests with In-Memory Database

```csharp
public class PostRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly PostRepository _repository;

    public PostRepositoryTests()
    {
        _testDb = new TestDbContext(); // unique in-memory SQLite DB
        _repository = new PostRepository(_testDb.Context);
    }

    public void Dispose() => _testDb.Dispose();
}
```

### RULE 55: Test Error Codes, Not Just Messages

```csharp
// Verifies the controller will map to the correct HTTP status
result.ErrorCode.Should().Be(ErrorCodes.NotFound);
result.ErrorCode.Should().Be(ErrorCodes.Forbidden);
```

---

## Naming Conventions

### RULE 56: C# Naming Standards

| Type | Convention | Example |
|------|-----------|---------|
| Classes | PascalCase | `PostService`, `UserRepository` |
| Interfaces | IPascalCase | `IPostService`, `IUserRepository` |
| Methods | PascalCase + Async | `CreatePostAsync`, `GetByIdAsync` |
| Private fields | _camelCase | `_unitOfWork`, `_logger` |
| Parameters | camelCase | `userId`, `postTitle` |
| ViewModels | PascalCase + ViewModel | `CreatePostViewModel` |
| DTOs | PascalCase + Dto/Response | `CreatePostDto`, `PostResponse` |
| Error Codes | UPPER_SNAKE | `NOT_FOUND`, `FORBIDDEN` |

### RULE 57: File Organization

```
Controllers/
├── Base/BaseController.cs           # MVC base class
├── AuthController.cs                # MVC - Authentication
├── DashboardController.cs           # MVC - Dashboard
├── HomeController.cs                # MVC - Landing page
├── ProfileController.cs             # MVC - Profile
├── ApiAuthController.cs             # API - Authentication
└── ApiPostsController.cs            # API - Posts

Application/Services/
├── AuthService.cs
└── PostService.cs

Core/
├── Common/Result.cs                 # Result<T>, ErrorCodes
├── Interfaces/                      # All interfaces
└── Validators/                      # Validation helpers

Infrastructure/
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Repositories/
│   ├── Repository.cs                # Generic base
│   ├── UserRepository.cs
│   └── PostRepository.cs
└── UnitOfWork.cs

DTOs/
├── RegisterRequest.cs
├── LoginRequest.cs
├── PostRequest.cs                   # CreatePostDto, UpdatePostDto
└── PostResponse.cs                  # PostResponse, PostUserResponse

Models/
├── User.cs
└── Post.cs
```

---

## New Feature Checklist

When adding a new feature, verify every item:

- [ ] **Model** created in `Models/` with proper annotations
- [ ] **Repository interface** extends `IRepository<T>` in `Core/Interfaces/`
- [ ] **Repository implementation** extends `Repository<T>` in `Infrastructure/Repositories/`
- [ ] **UnitOfWork** updated with new repository property
- [ ] **Service interface** in `Core/Interfaces/` returns `Result<T>`
- [ ] **Service implementation** in `Application/Services/` with validation and error codes
- [ ] **Request DTOs** with data annotations in `DTOs/`
- [ ] **Response DTOs** with `FromModel()` factory in `DTOs/`
- [ ] **Controller** with try-catch, `MapError`, response DTOs
- [ ] **DI registration** in `Program.cs`
- [ ] **Tests** in proper subdirectory with FluentAssertions
- [ ] **All 56 tests pass** (`dotnet test`)
- [ ] **Build succeeds** (`dotnet build`)

---

## Enforcement

These are mandatory standards. All pull requests must:

1. Follow all rules in this document
2. Include tests for all new/modified features
3. All tests pass (`dotnet test`)
4. Build without errors
5. Pass code review

**PRs without tests = automatic rejection.**
**Failing tests = PR blocked.**

---

**Last Updated:** April 15, 2026
**Version:** 3.0
