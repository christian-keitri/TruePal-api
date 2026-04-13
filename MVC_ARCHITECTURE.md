# Scalable MVC Architecture

## 📁 MVC Folder Structure

Your application now follows the **Model-View-Controller (MVC)** pattern with feature-based organization:

```
Controllers/
├── Base/                          # Base classes for inheritance
│   └── BaseController.cs         # Shared controller functionality
│
├── AuthController.cs             # Authentication controller (Login, Register, Logout)
├── DashboardController.cs        # Dashboard controller
├── ProfileController.cs          # Profile controller
├── HomeController.cs             # Home page controller
│
├── ApiAuthController.cs          # REST API - Authentication
├── ApiPostsController.cs         # REST API - Posts
└── ApiUsersController.cs         # REST API - Users

Views/
├── Auth/                          # Authentication views
│   ├── Login.cshtml              # Login form view
│   ├── Register.cshtml           # Registration form view
│   └── ForgotPassword.cshtml     # Password reset view
│
├── Dashboard/                     # Dashboard views
│   └── Index.cshtml              # Dashboard main view
│
├── Profile/                       # Profile views
│   └── Index.cshtml              # Profile main view
│
├── Home/                          # Home views
│   └── Index.cshtml              # Landing page with map
│
├── Shared/                        # Shared layouts and components
│   ├── _Layout.cshtml            # Main layout template
│   ├── _StatusMessages.cshtml    # Reusable alert component
│   ├── _ValidationScriptsPartial.cshtml  # Client-side validation
│   └── Components/               # Reusable component partials
│       ├── _AuthOverlay.cshtml   # Auth call-to-action overlay
│       ├── _MapOverlay.cshtml    # Map header overlay
│       └── _PostsPanel.cshtml    # Posts slide-out panel
│
├── _ViewImports.cshtml           # Global imports and namespaces
└── _ViewStart.cshtml             # Default layout configuration
```

## 🎯 Key Improvements

### 1. Feature-Based Organization
**Before (Razor Pages):**
```
Pages/
├── Login.cshtml
├── Register.cshtml
├── _Layout.cshtml
```

**After (MVC):**
```
Controllers/
├── AuthController.cs      # All auth actions in one controller
└── DashboardController.cs # All dashboard actions in one controller

Views/
├── Auth/           # All auth-related views together
│   ├── Login.cshtml
│   └── Register.cshtml
├── Dashboard/      # All dashboard views together
└── Shared/         # Shared components
```

**Benefits:**
- Related features grouped together
- Easy to find and maintain
- Scales as you add more features
- Clear controller responsibilities
- Testable controller actions

### 2. ViewModels with Data Annotations

ViewModels are defined within the controller file for easy access:

```csharp
// At bottom of AuthController.cs
public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
```

**Benefits:**
- Centralized validation rules
- Reusable across actions
- Type-safe with IntelliSense
- Automatic client-side validation
- Clear data contracts

### 3. BaseController Class

**Before:**
```csharp
public class AuthController : Controller
{
    public string? ErrorMessage { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    // Repeat in every controller...
}
```

**After:**
```csharp
public class AuthController : BaseController
{
    public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger) 
        : base(logger, configuration)
    {
    }
    
    // ErrorMessage, ErrorMessages, SuccessMessage, etc. inherited via TempData
    // Helper methods available: SetError(), SetSuccess(), RedirectToActionWithSuccess()
}
```

**Benefits:**
- DRY (Don't Repeat Yourself)
- Consistent error handling
- Centralized logging
- Built-in helper methods
- Easy to extend with new functionality

### 4. Shared Components

**_StatusMessages.cshtml** - Reusable alert component:
```razor
<partial name="_StatusMessages" />
```

Displays success, error, info, and warning messages consistently across all pages.

**_ValidationScriptsPartial.cshtml** - Client-side validation:
```razor
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

Enables real-time form validation before submission.

### 5. Enhanced Layout with Icons

The new layout includes:
- Bootstrap Icons for visual appeal
- Conditional navigation (logged in vs not)
- Status message display
- Sticky footer
- Responsive design
- Modern gradient background

## 🔧 How to Use

### Creating a New Feature

Example: Adding a "Posts" feature

1. **Create controller:**
```csharp
// Controllers/PostsController.cs
namespace TruePal.Api.Controllers;

public class PostsController : BaseController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService, IConfiguration configuration, ILogger<PostsController> logger) 
        : base(logger, configuration)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = await _postService.GetAllPostsAsync();
        return View(posts);
    }

    [HttpGet]
    public IActionResult Create()
### Using BaseController Helper Methods

```csharp
// Set success message (stored in TempData)
SetSuccess("Operation completed!");

// Set error message
SetError("Something went wrong");

// Set info message
SetInfo("Please note this information");

// Redirect with success message
return RedirectToActionWithSuccess("Index", "Dashboard", "Profile saved!");

// Add multiple errors to ModelState
AddErrors(result.Errors);

// Log and display error
LogAndDisplayError("Database connection failed", exception);
```

### Creating Reusable Components

```razor
@* Views/Shared/Components/_UserCard.cshtml *@
@model User

<div class="card">
    <div class="card-body">
        <h5>@Model.Username</h5>
        <p>@Model.Email</p>
    </div>
</div>

@* Usage in any view: *@
<partial name="Components/urn Page();

        var result = await _profileService.UpdateProfileAsync(Input);
        
        if (!result.IsSuccess)
        {
            AddErrors(result.Errors);
            return Page();
        }

        return RedirectToPageWithSuccess("/Profile/Index", "Profile updated successfully!");
    }
}
```

### Using BasePageModel Helper Methods

```csharp
// Add a single error
AddError("Something went wrong");

// Add multiple errors
AddErrors(result.Errors);

// Set success message
SetSuccess("Operation completed!");

// Redirect with success message
return RedirectToPageWithSuccess("/Dashboard/Index", "Profile saved!");

// Redirect with error message
return RedirectToPageWithError("/Profile/Edit", "Invalid data");

// Log and display error
LogAndDisplayError("Database connection failed", exception);
```

### Creating Reusable Components

```razor
@* Pages/Shared/_UserCard.cshtml *@
@model User

<div class="card">
    <div class="card-body">
        <h5>@Model.Username</h5>
        <p>@Model.Email</p>
    </div>
</div>

@* Usage in any page: *@
<partial name="_UserCard" model="user" />
```

## 📊 Validation Features

### Server-Side Validation
Handled by Data Annotations in ViewModels:
```csharp
[Required(ErrorMessage = "Email is required")]
[EmailAddress(ErrorMessage = "Invalid email format")]
public string Email { get; set; } = string.Empty;
```

### Client-Side Validation
Automatically enabled with `_ValidationScriptsPartial`:
- Real-time validation as user types
- Prevents form submission if invalid
- Shows error messages inline

### Custom Validation
Add custom validation attributes:
```csharp
public class UsernameAvailableAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        // Check if username is available
        // Return ValidationResult.Success or new ValidationResult("Error message")
    }
}

// Usage
[UsernameAvailable]
public string Username { get; set; }
```

## 🎨 UI Consistency

All pages now have:
- **Consistent styling** - Same Bootstrap theme
- **Icon support** - Bootstrap Icons throughout
- **Responsive design** - Mobile-friendly
- **Dismissible alerts** - User can close messages
- **Loading states** - Can add spinners easily
- **Validation feedback** - Red underlines for errors

## 🔐 Security Features

- **CSRF Protection** - Automatic with [ValidateAntiForgeryToken]
- **Input Validation** - Multiple layers (client + server)
- **XSS Prevention** - Razor escapes output by default
- **Authentication Checks** - Easy with base class methods

## 📈 Scalability Benefits

### Easy to Add Features
Adding a new feature requires only:
1. Create feature folder
2. Add ViewModels
3. Create pages inheriting from BasePageModel
4. Follow established patterns

### Easy to Test
```csharp
[Fact]
public async Task Login_WithInvalidCredentials_ShowsError()
{
    // Arrange
    var mockAuthService = new Mock<IAuthService>();
    mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(Result<string>.Failure("Invalid credentials"));
    
    var model = new LoginModel(mockAuthService.Object, logger);
    
    // Act
    var result = await model.OnPostAsync();
    
    // Assert
    Assert.True(model.HasErrors);
}
```

### Easy to Maintain
- Clear structure - know where everything goes
- Consistent patterns - same approach everywhere
- Reusable components - build once, use everywhere
- Centralized logic - changes in one place

## 🚀 Next Steps

1. **Add Authorization**
   - Create `[Authorize]` attribute handler
   - Add role-based access control
   - Protect sensitive pages

2. **Add More Features**
   - User Profile management
   - Friend requests
   - Posts and comments
   - Notifications

3. **Enhance UI**
   - Add loading spinners
   - Add confirmation modals
   - Add tooltips
   - Add animations

4. **Add Analytics**
   - Track page views
   - Monitor user actions
   - Performance metrics

## 📝 Conventions

- **Feature folders** - Group by feature, not type
- **ViewModels** - Suffix with `ViewModel`
- **Page handlers** - Use OnGet/OnPost naming
- **Services** - Inject via constructor
- **Logging** - Use provided _logger
- **Messages** - Use BasePageModel helpers
- **Validation** - Data Annotations + custom logic

Your Pages folder is now production-ready and scalable! 🎉
