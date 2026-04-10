# Scalable Razor Pages Architecture

## 📁 New Pages Folder Structure

Your Pages folder is now organized with scalable patterns following feature-based organization:

```
Pages/
├── Base/                          # Base classes for inheritance
│   └── BasePageModel.cs          # Shared page model functionality
│
├── ViewModels/                    # Data Transfer Objects for pages
│   ├── LoginViewModel.cs         # Login form data with validation
│   └── RegisterViewModel.cs      # Registration form data with validation
│
├── Auth/                          # Authentication feature folder
│   ├── Login.cshtml              # Login page view
│   ├── Login.cshtml.cs           # Login page logic
│   ├── Register.cshtml           # Registration page view
│   ├── Register.cshtml.cs        # Registration page logic
│   ├── ForgotPassword.cshtml     # Password reset view
│   └── ForgotPassword.cshtml.cs  # Password reset logic
│
├── Dashboard/                     # Dashboard feature folder
│   ├── Index.cshtml              # Dashboard view
│   └── Index.cshtml.cs           # Dashboard logic
│
├── Shared/                        # Shared layouts and components
│   ├── _Layout.cshtml            # Main layout template
│   ├── _StatusMessages.cshtml    # Reusable alert component
│   └── _ValidationScriptsPartial.cshtml  # Client-side validation
│
├── Index.cshtml                   # Home page view
├── Index.cshtml.cs               # Home page logic
├── _ViewImports.cshtml           # Global imports
└── _ViewStart.cshtml             # Default layout configuration
```

## 🎯 Key Improvements

### 1. Feature-Based Organization
**Before:**
```
Pages/
├── Login.cshtml
├── Register.cshtml
├── _Layout.cshtml
```

**After:**
```
Pages/
├── Auth/           # All auth-related pages together
│   ├── Login.cshtml
│   └── Register.cshtml
├── Dashboard/      # All dashboard pages together
└── Shared/         # Shared components
```

**Benefits:**
- Related features grouped together
- Easy to find and maintain
- Scales as you add more features
- Clear ownership boundaries

### 2. ViewModels with Data Annotations

**Before:**
```csharp
[BindProperty]
public string Email { get; set; } = string.Empty;

[BindProperty]
public string Password { get; set; } = string.Empty;
```

**After:**
```csharp
[BindProperty]
public LoginViewModel Input { get; set; } = new();

// LoginViewModel.cs
public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```

**Benefits:**
- Centralized validation rules
- Reusable across pages and APIs
- Type-safe with IntelliSense
- Automatic client-side validation
- Clear data contracts

### 3. Base PageModel Class

**Before:**
```csharp
public class LoginModel : PageModel
{
    public string? ErrorMessage { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    // Repeat in every page...
}
```

**After:**
```csharp
public class LoginModel : BasePageModel
{
    public LoginModel(IAuthService authService, ILogger<LoginModel> logger) 
        : base(logger)
    {
    }
    
    // ErrorMessage, ErrorMessages, SuccessMessage, etc. inherited
    // Helper methods available: AddError(), SetSuccess(), LogAndDisplayError()
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

### Creating a New Feature Folder

Example: Adding a "Profile" feature

1. **Create folder structure:**
```
Pages/
└── Profile/
    ├── Index.cshtml          # View profile
    ├── Index.cshtml.cs       
    ├── Edit.cshtml           # Edit profile
    └── Edit.cshtml.cs
```

2. **Create ViewModel:**
```csharp
// Pages/ViewModels/ProfileViewModel.cs
public class ProfileViewModel
{
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Bio { get; set; } = string.Empty;
}
```

3. **Create PageModel:**
```csharp
// Pages/Profile/Edit.cshtml.cs
namespace TruePal.Api.Pages.Profile;

public class EditModel : BasePageModel
{
    private readonly IProfileService _profileService;

    public EditModel(IProfileService profileService, ILogger<EditModel> logger) 
        : base(logger)
    {
        _profileService = profileService;
    }

    [BindProperty]
    public ProfileViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Load profile data
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

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
