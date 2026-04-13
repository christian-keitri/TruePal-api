# TruePal.Api - Coding Standards & Development Rules

## 🎯 Purpose
This document defines the mandatory coding patterns and reusable components for TruePal.Api development. Following these rules ensures consistency, maintainability, and leverages the established architecture.

---

## 📋 Table of Contents
1. [MVC Pattern Rules](#mvc-pattern-rules)
2. [Controller Standards](#controller-standards)
3. [View Standards](#view-standards)
4. [Service Layer Rules](#service-layer-rules)
5. [Repository Pattern Rules](#repository-pattern-rules)
6. [Error Handling Rules](#error-handling-rules)
7. [Validation Rules](#validation-rules)
8. [CSS & Component Rules](#css--component-rules)
9. [Security Rules](#security-rules)
10. [Naming Conventions](#naming-conventions)

---

## 🏗️ MVC Pattern Rules

### ✅ RULE 1: Always Use Strongly-Typed ViewModels
**❌ BAD - ViewData (Type-unsafe)**
```csharp
// Controller
ViewData["Username"] = user.Username;
ViewData["Email"] = user.Email;

// View
var username = ViewData["Username"] as string ?? "";
```

**✅ GOOD - ViewModel (Type-safe)**
```csharp
// Controller
var model = new ProfileViewModel 
{
    Username = user.Username,
    Email = user.Email
};
return View(model);

// View
@model ProfileViewModel
<h1>@Model.Username</h1>
```

**Why:** Type safety, IntelliSense, compile-time checking, refactoring support.

---

### ✅ RULE 2: Define ViewModels in Controller Files
**Location:** At the bottom of controller files in a `#region ViewModels` block

**✅ CORRECT Structure:**
```csharp
namespace TruePal.Api.Controllers;

public class PostsController : BaseController
{
    // Controller actions here
}

#region ViewModels

public class CreatePostViewModel
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
}

#endregion
```

**Why:** Co-location keeps related code together, easier to find and maintain.

---

### ✅ RULE 3: All Controllers Must Inherit from BaseController
**❌ BAD**
```csharp
public class PostsController : Controller
{
    private readonly ILogger<PostsController> _logger;
    
    public PostsController(ILogger<PostsController> logger)
    {
        _logger = logger;
    }
}
```

**✅ GOOD**
```csharp
public class PostsController : BaseController
{
    public PostsController(IConfiguration configuration, ILogger<PostsController> logger) 
        : base(logger, configuration)
    {
    }
}
```

**Why:** Access to helper methods (SetSuccess, SetError, LogAndDisplayError), consistent logging, configuration access.

---

## 🎛️ Controller Standards

### ✅ RULE 4: Use BaseController Helper Methods
**Available Methods:**
- `SetSuccess(string message)` - Success message via TempData
- `SetError(string message)` - Error message via TempData
- `SetInfo(string message)` - Info message via TempData
- `RedirectToActionWithSuccess(action, controller, message)` - Redirect with success
- `AddErrors(List<string> errors)` - Add multiple errors to ModelState
- `LogAndDisplayError(message, exception)` - Log and show user-friendly error

**❌ BAD**
```csharp
TempData["SuccessMessage"] = "Post created!";
return RedirectToAction("Index");
```

**✅ GOOD**
```csharp
return RedirectToActionWithSuccess("Index", "Posts", "Post created successfully!");
```

---

### ✅ RULE 5: Always Check Authentication Before Accessing Protected Resources
**✅ REQUIRED Pattern:**
```csharp
public IActionResult Index()
{
    // First: Check authentication
    if (!Request.Cookies.ContainsKey("AuthToken"))
    {
        return RedirectToActionWithError("Login", "Auth", "Please login to continue");
    }
    
    // Then: Load data and return view
    var model = new DashboardViewModel { ... };
    return View(model);
}
```

---

### ✅ RULE 6: Use ValidateAntiForgeryToken for All POST Actions
**✅ REQUIRED:**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreatePostViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    
    // Process...
}
```

**Why:** Prevents CSRF attacks.

---

## 👁️ View Standards

### ✅ RULE 7: Always Declare @model At Top of View
**✅ REQUIRED:**
```cshtml
@model CreatePostViewModel
@{
    ViewData["Title"] = "Create Post";
}

<h1>Create New Post</h1>
```

---

### ✅ RULE 8: Use Reusable Partial Components
**Available Partials:**
- `<partial name="Components/_MapOverlay" />` - Map header overlay
- `<partial name="Components/_AuthOverlay" />` - Auth call-to-action
- `<partial name="Components/_PostsPanel" />` - Posts slide-out panel
- `<partial name="_StatusMessages" />` - Success/Error/Info messages

**✅ ALWAYS Include in Layouts:**
```cshtml
@* In _Layout.cshtml or views *@
<partial name="_StatusMessages" />
```

**Why:** Displays messages from TempData (SetSuccess, SetError, etc.).

---

### ✅ RULE 9: Use Component CSS Classes, Not Inline Styles
**❌ BAD**
```html
<div style="background: yellow; padding: 20px;">...</div>
```

**✅ GOOD**
```html
<div class="flex-card">...</div>
```

**Available Component Classes:**
- `.flex-card` - Reusable card component
- `.posts-panel` - Slide-out panel
- `.overlay-content` - Overlay container

**See:** [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md) for all available classes.

---

### ✅ RULE 10: Load CSS Per-Page Using @section Styles
**✅ REQUIRED Pattern:**
```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/components/cards.css" asp-append-version="true">
    <link rel="stylesheet" href="~/css/pages/posts.css" asp-append-version="true">
}
```

**Why:** Only load what's needed, faster page loads.

---

## 🔧 Service Layer Rules

### ✅ RULE 11: Always Use Result Pattern for Service Methods
**❌ BAD - Exceptions for control flow**
```csharp
public async Task<User> CreateUserAsync(string email)
{
    if (await _userRepository.EmailExistsAsync(email))
        throw new Exception("Email exists");
    
    return user;
}
```

**✅ GOOD - Result Pattern**
```csharp
public async Task<Result<User>> CreateUserAsync(string email)
{
    if (await _userRepository.EmailExistsAsync(email))
        return Result<User>.Failure("Email already exists");
    
    return Result<User>.Success(user);
}
```

**Why:** Type-safe error handling, no exceptions for business logic, clearer intent.

---

### ✅ RULE 12: Services Must Use Interfaces
**❌ BAD - Concrete class**
```csharp
public class PostsController : BaseController
{
    private readonly PostService _postService; // ❌ Concrete
}
```

**✅ GOOD - Interface**
```csharp
public class PostsController : BaseController
{
    private readonly IPostService _postService; // ✅ Interface
}
```

**Why:** Testability, dependency inversion, loose coupling.

---

### ✅ RULE 13: Use Dependency Injection for All Dependencies
**✅ REQUIRED Pattern:**
```csharp
public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostService> _logger;
    
    public PostService(IUnitOfWork unitOfWork, ILogger<PostService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
}
```

**Register in Program.cs:**
```csharp
builder.Services.AddScoped<IPostService, PostService>();
```

---

## 🗄️ Repository Pattern Rules

### ✅ RULE 14: Always Access Database Through UnitOfWork
**❌ BAD - Direct DbContext**
```csharp
public class PostService
{
    private readonly AppDbContext _context; // ❌ Direct access
    
    public async Task CreatePost(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
    }
}
```

**✅ GOOD - UnitOfWork Pattern**
```csharp
public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork; // ✅ Through UnitOfWork
    
    public async Task<Result<Post>> CreatePostAsync(Post post)
    {
        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.CompleteAsync();
        return Result<Post>.Success(post);
    }
}
```

**Why:** Transaction management, atomic operations, abstraction.

---

### ✅ RULE 15: Repositories Must Implement Generic IRepository<T>
**✅ REQUIRED Pattern:**
```csharp
public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetByUserIdAsync(int userId);
    Task<bool> TitleExistsAsync(string title);
}

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context) { }
    
    public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
    {
        return await _context.Posts.Where(p => p.UserId == userId).ToListAsync();
    }
}
```

**Why:** Inherits common operations (GetByIdAsync, GetAllAsync, etc.), reduces code duplication.

---

## ⚠️ Error Handling Rules

### ✅ RULE 16: Never Expose Raw Exception Messages to Users
**❌ BAD**
```csharp
catch (Exception ex)
{
    SetError(ex.Message); // ❌ Exposes internal details
}
```

**✅ GOOD**
```csharp
catch (Exception ex)
{
    LogAndDisplayError("An error occurred while creating the post. Please try again.", ex);
}
```

**Why:** Security, user experience, log details internally.

---

### ✅ RULE 17: Use Try-Catch in Controllers, Not Services
**✅ CORRECT Pattern:**
```csharp
// Controller
public async Task<IActionResult> Create(CreatePostViewModel model)
{
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
}

// Service - Returns Result, doesn't throw
public async Task<Result<Post>> CreatePostAsync(CreatePostViewModel model)
{
    // Business logic, returns Result<Post>
}
```

---

## ✔️ Validation Rules

### ✅ RULE 18: Use Data Annotations for ViewModel Validation
**✅ REQUIRED:**
```csharp
public class CreatePostViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be 5-100 characters")]
    [Display(Name = "Post Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 10)]
    [Display(Name = "Post Content")]
    public string Content { get; set; } = string.Empty;
}
```

**Why:** Client and server-side validation, clear error messages, reusable.

---

### ✅ RULE 19: Always Check ModelState.IsValid
**✅ REQUIRED Pattern:**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreatePostViewModel model)
{
    if (!ModelState.IsValid)
        return View(model); // ✅ Return view with validation errors
    
    // Process...
}
```

---

### ✅ RULE 20: Include Validation Scripts in Views with Forms
**✅ REQUIRED:**
```cshtml
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

**Why:** Enables client-side validation (jQuery Validation).

---

## 🎨 CSS & Component Rules

### ✅ RULE 21: Follow Component-Based CSS Architecture
**Structure:**
```
wwwroot/css/
├── theme.css           # ✅ Global theme (always loaded)
├── components/         # ✅ Reusable components
│   ├── cards.css
│   ├── panels.css
│   └── overlays.css
└── pages/              # ✅ Page-specific styles
    ├── home.css
    └── posts.css
```

**Loading Order:**
1. `theme.css` - Always in _Layout.cshtml
2. Component CSS - Per page as needed
3. Page CSS - Per page

---

### ✅ RULE 22: Use CSS Variables from Theme
**❌ BAD**
```css
.my-card {
    background: #c9a961; /* ❌ Hardcoded */
    padding: 16px;
}
```

**✅ GOOD**
```css
.my-card {
    background: var(--primary-yellow); /* ✅ CSS variable */
    padding: var(--spacing-md);
}
```

**Available Variables:** See [THEME_GUIDE.md](THEME_GUIDE.md)

---

### ✅ RULE 23: Create Reusable Components, Not Page-Specific Duplicates
**If a component is used in 2+ places, make it reusable:**

1. Create CSS in `wwwroot/css/components/`
2. Create partial in `Views/Shared/Components/`
3. Document in CSS_ARCHITECTURE.md

---

## 🔐 Security Rules

### ✅ RULE 24: Always Use HttpOnly, Secure Cookies for Authentication
**✅ REQUIRED Pattern:**
```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,    // ✅ Prevents XSS access
    Secure = true,      // ✅ HTTPS only
    SameSite = SameSiteMode.Strict, // ✅ CSRF protection
    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
};

Response.Cookies.Append("AuthToken", token, cookieOptions);
```

---

### ✅ RULE 25: Always Escape User-Generated Content in JavaScript
**❌ BAD**
```javascript
element.innerHTML = userInput; // ❌ XSS vulnerability
```

**✅ GOOD**
```javascript
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

element.innerHTML = escapeHtml(userInput); // ✅ Safe
```

---

### ✅ RULE 26: Never Store Passwords in Plain Text
**✅ REQUIRED: Use BCrypt**
```csharp
// Hash password
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

// Verify password
bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
```

---

## 📛 Naming Conventions

### ✅ RULE 27: Follow C# Naming Standards
| Type | Convention | Example |
|------|-----------|---------|
| Classes | PascalCase | `PostService`, `UserRepository` |
| Interfaces | IPascalCase | `IPostService`, `IUserRepository` |
| Methods | PascalCase | `CreatePostAsync`, `GetUserByIdAsync` |
| Private fields | _camelCase | `_unitOfWork`, `_logger` |
| Parameters | camelCase | `userId`, `postTitle` |
| ViewModels | PascalCase + "ViewModel" | `CreatePostViewModel` |

---

### ✅ RULE 28: Async Method Naming
**✅ All async methods must end with "Async":**
```csharp
public async Task<Result<Post>> CreatePostAsync(Post post)
public async Task<User?> GetUserByIdAsync(int id)
```

---

### ✅ RULE 29: Controller Action Naming
**Use HTTP verb naming:**
```csharp
[HttpGet]
public IActionResult Create() { } // Shows form

[HttpPost]
public IActionResult Create(CreatePostViewModel model) { } // Processes form
```

---

### ✅ RULE 30: File Organization
**Controllers:** `Controllers/FeatureController.cs`
```
Controllers/
├── AuthController.cs
├── PostsController.cs
├── ProfileController.cs
```

**Views:** `Views/Feature/Action.cshtml`
```
Views/
├── Auth/
│   ├── Login.cshtml
│   └── Register.cshtml
├── Posts/
│   ├── Index.cshtml
│   └── Create.cshtml
```

---

## 🚀 Quick Checklist for New Features

When adding a new feature, follow this checklist:

- [ ] **Controller**: Inherits from BaseController
- [ ] **ViewModel**: Defined in controller file with `#region ViewModels`
- [ ] **View**: Uses `@model YourViewModel`
- [ ] **Service**: Interface in `Core/Interfaces/`, implementation in `Application/Services/`
- [ ] **Repository**: Interface extends `IRepository<T>`, implementation extends `Repository<T>`
- [ ] **Validation**: Data annotations on ViewModel properties
- [ ] **Error Handling**: Try-catch in controller, returns Result<T> from service
- [ ] **Authentication**: Check `AuthToken` cookie before accessing protected resources
- [ ] **CSRF Protection**: `[ValidateAntiForgeryToken]` on POST actions
- [ ] **CSS**: Component styles in `wwwroot/css/components/`, page styles in `pages/`
- [ ] **Security**: User input escaped, passwords hashed, cookies HttpOnly+Secure
- [ ] **DI Registration**: Service registered in `Program.cs`

---

## 📚 Related Documentation

- [ARCHITECTURE.md](ARCHITECTURE.md) - Overall architecture patterns
- [MVC_ARCHITECTURE.md](MVC_ARCHITECTURE.md) - MVC structure and best practices
- [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md) - CSS organization
- [THEME_GUIDE.md](THEME_GUIDE.md) - Design system and CSS variables
- [QUICKSTART.md](QUICKSTART.md) - Running the application
- [MVC_MIGRATION.md](MVC_MIGRATION.md) - Understanding the MVC migration

---

## ⚖️ Enforcement

**These are MANDATORY standards.** All pull requests must:
1. ✅ Follow these coding standards
2. ✅ Use existing reusable components
3. ✅ Pass code review checklist
4. ✅ Build without errors
5. ✅ Include appropriate documentation updates

**Non-compliance will result in PR rejection.**

---

**Last Updated:** April 13, 2026  
**Version:** 1.0  
**Maintained by:** TruePal Development Team
