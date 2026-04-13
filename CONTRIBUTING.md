# Contributing to TruePal.Api

## 🎯 Before You Start

**MANDATORY READING:**
1. **[CODING_STANDARDS.md](CODING_STANDARDS.md)** - Code quality rules
2. **[UI_UX_STANDARDS.md](UI_UX_STANDARDS.md)** - Design and UI rules

All contributions must follow the established coding standards and design patterns.

---

## 🚦 Development Workflow

### 1. Setup Your Environment
```bash
# Clone the repository
git clone [repository-url]

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

### 2. Read the Documentation
- **[CODING_STANDARDS.md](CODING_STANDARDS.md)** ⭐ **REQUIRED** - Development rules (30 rules)
- **[UI_UX_STANDARDS.md](UI_UX_STANDARDS.md)** ⭐ **REQUIRED** - Design rules (36 rules)
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Understanding the architecture
- **[MVC_ARCHITECTURE.md](MVC_ARCHITECTURE.md)** - MVC patterns

### 3. Follow the Patterns
✅ **Use existing reusable code:**
- Inherit from `BaseController`
- Use `Result<T>` pattern for service methods
- Access database through `IUnitOfWork`
- Use strongly-typed ViewModels
- Use existing CSS components

### 4. Create Your Feature Branch
```bash
git checkout -b feature/your-feature-name
```

### 5. Write Code Following Standards
Refer to [CODING_STANDARDS.md](CODING_STANDARDS.md) for:
- MVC patterns
- Controller standards
- Service layer rules
- Repository patterns
- Error handling
- Validation
- Security rules
- Naming conventions

### 6. Write/Update Tests (MANDATORY)
**⚠️ REQUIRED: All features MUST include tests**

```bash
# Navigate to test project
cd TruePal.Api.Tests

# Run existing tests to ensure nothing broke
dotnet test

# Create tests for your new feature in appropriate folder:
# - Repositories/ for repository tests
# - Services/ for service tests  
# - Integration/ for end-to-end tests
```

**Minimum Test Requirements:**
- ✅ At least 1 test for success case
- ✅ At least 1 test for failure/validation case
- ✅ Follow Arrange-Act-Assert pattern
- ✅ Use descriptive test names: `MethodName_ExpectedBehavior_StateUnderTest`

**Example:**
```csharp
[Fact]
public async Task CreateUser_ShouldSucceed_WhenValidDataProvided()
{
    // Arrange
    var user = new User { Email = "test@example.com" };
    
    // Act
    await _repository.AddAsync(user);
    await _context.SaveChangesAsync();
    
    // Assert
    user.Id.Should().BeGreaterThan(0);
}
```

### 7. Test Your Changes
```bash
# Run all tests (MANDATORY before commit)
cd TruePal.Api.Tests
dotnet test
# ✅ All tests must pass

# Build the project
cd ../
dotnet build

# Run the application
dotnet run

# Manually test all affected pages
```

### 8. Commit Your Changes
```bash
git add .
git commit -m "feat: descriptive commit message"
```

**Commit Message Format:**
- `feat:` New feature
- `fix:` Bug fix
- `refactor:` Code refactoring
- `docs:` Documentation changes
- `style:` Code style changes (formatting)
- `test:` Adding tests

### 9. Submit Pull Request
Before submitting, verify:
- [ ] **Tests Added/Updated** - MANDATORY for all features ⚠️
- [ ] **All Tests Pass** - Run `dotnet test` successfully ⚠️
- [ ] **Test Coverage** - Minimum requirements met (see CODING_STANDARDS.md Rule 28) ⚠️
- [ ] Follows [CODING_STANDARDS.md](CODING_STANDARDS.md) (38 rules)
- [ ] Follows [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md)
- [ ] Uses existing reusable components
- [ ] Builds without errors
- [ ] All pages tested manually
- [ ] Responsive on mobile (375px) to desktop (1920px)
- [ ] No hardcoded CSS values (uses variables)
- [ ] Accessible (WCAG AA compliant)
- [ ] No exposed sensitive data
- [ ] ViewModels are strongly-typed
- [ ] Services return `Result<T>`
- [ ] Controllers inherit from `BaseController`

---

## 📋 Pull Request Checklist

### Code Quality
- [ ] Follows coding standards
- [ ] Uses dependency injection
- [ ] Proper error handling
- [ ] Input validation implemented
- [ ] No hardcoded values
- [ ] Uses CSS variables from theme

### UI/UX Quality
- [ ] Responsive design (mobile 375px to desktop 1920px)
- [ ] Uses CSS variables (no hardcoded colors/spacing)
- [ ] Bootstrap components used correctly
- [ ] Proper hover/focus states
- [ ] Accessible (ARIA labels, alt text, keyboard navigation)
- [ ] Meets WCAG AA contrast requirements
- [ ] Bootstrap Icons only (no mixed icon sets)
- [ ] Mobile-first CSS approach

### Security
- [ ] No passwords in plain text
- [ ] User input escaped
- [ ] Authentication checked
- [ ] CSRF protection enabled
- [ ] HttpOnly+Secure cookies

### Architecture
- [ ] Controller inherits from BaseController
- [ ] ViewModel defined in controller file
- [ ] Service uses Result<T> pattern
- [ ] Repository accessed via UnitOfWork
- [ ] Follows MVC pattern

### Documentation
- [ ] Code comments where needed
- [ ] README updated if needed
- [ ] New components documented in CSS_ARCHITECTURE.md

---

## 🚫 Common Mistakes to Avoid

### ❌ Don't Use ViewData/ViewBag
```csharp
// ❌ BAD
ViewData["Username"] = user.Username;

// ✅ GOOD
var model = new ProfileViewModel { Username = user.Username };
return View(model);
```

### ❌ Don't Inherit Directly from Controller
```csharp
// ❌ BAD
public class PostsController : Controller

// ✅ GOOD
public class PostsController : BaseController
```

### ❌ Don't Access DbContext Directly
```csharp
// ❌ BAD
_context.Posts.Add(post);

// ✅ GOOD
await _unitOfWork.Posts.AddAsync(post);
```

### ❌ Don't Throw Exceptions for Business Logic
```csharp
// ❌ BAD
if (exists) throw new Exception("User exists");

// ✅ GOOD
if (exists) return Result<User>.Failure("User already exists");
```

### ❌ Don't Use Inline Styles
```html
<!-- ❌ BAD -->
<div style="padding: 20px;">

<!-- ✅ GOOD -->
<div class="flex-card">
```

---

## 🆘 Getting Help

### Documentation Resources
1. [CODING_STANDARDS.md](CODING_STANDARDS.md) - Code quality rules
2. [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md) - Design and UI rules
3. [ARCHITECTURE.md](ARCHITECTURE.md) - Architecture overview
4. [MVC_ARCHITECTURE.md](MVC_ARCHITECTURE.md) - MVC patterns
5. [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md) - CSS organization
6. [THEME_GUIDE.md](THEME_GUIDE.md) - CSS variables reference

### Questions?
- Check existing documentation first
- Review similar existing code
- Ask the team lead

---

## 🎓 Learning Path for New Developers

### Week 1: Understanding
1. Read [README.md](README.md)
2. Read [QUICKSTART.md](QUICKSTART.md)
3. Run the application
4. Explore existing features

### Week 2: Architecture
1. Read [ARCHITECTURE.md](ARCHITECTURE.md)
2. Read [MVC_ARCHITECTURE.md](MVC_ARCHITECTURE.md)
3. Study existing controllers
4. Understand Result pattern

### Week 3: Standards
1. Read [CODING_STANDARDS.md](CODING_STANDARDS.md) thoroughly
2. Read [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md) thoroughly
3. Review all coding rules (30 rules)
4. Review all UI/UX rules (36 rules)
5. Study code examples
6. Practice with small changes

### Week 4: Contributing
1. Pick a small feature
2. Follow coding standards
3. Submit first PR
4. Iterate based on feedback

---

## 📝 Code Review Process

All PRs will be reviewed for:
1. **Adherence to coding standards** (blocking)
2. **Security vulnerabilities** (blocking)
3. **Architecture compliance** (blocking)
4. **Performance issues** (advisory)
5. **Code quality** (advisory)

**Reviewers will reject PRs that violate [CODING_STANDARDS.md](CODING_STANDARDS.md).**

---

## ✅ Definition of Done

A feature is "done" when:
- [ ] Code follows all coding standards
- [ ] Uses existing reusable components
- [ ] Builds without errors or warnings
- [ ] Manually tested and working
- [ ] Security review passed
- [ ] Code review approved
- [ ] Documentation updated
- [ ] Merged to main branch

---

**Thank you for contributing to TruePal.Api!** 🎉

By following these guidelines, you help maintain a high-quality, consistent, and maintainable codebase.
