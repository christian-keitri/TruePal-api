# TruePal.Api - Scalable Architecture

## 🏗️ Architecture Overview

This project follows a **Clean Architecture** pattern with clear separation of concerns:

### Layers

```
TruePal.Api/
├── Core/                      # Domain & Application Core
│   ├── Common/               # Shared classes (Result pattern)
│   ├── Configuration/         # Configuration models
│   ├── Interfaces/           # Service & Repository interfaces
│   └── Validators/           # Business logic validation
│
├── Application/              # Application Services
│   └── Services/             # Business logic implementation
│
├── Infrastructure/           # External concerns
│   ├── Middleware/           # Global middleware
│   └── Repositories/         # Data access implementation
│
├── Data/                     # Database context
├── Models/                   # Domain entities
├── DTOs/                     # Data transfer objects
├── Controllers/              # API endpoints
└── Pages/                    # Razor Pages UI
```

## 🔑 Key Design Patterns

### 1. **Repository Pattern**
Abstracts data access logic from business logic
- `IRepository<T>` - Generic repository interface
- `IUserRepository` - Specific user operations
- `UserRepository` - Implementation with EF Core

### 2. **Unit of Work Pattern**
Manages transactions across multiple repositories
- `IUnitOfWork` - Coordinates repositories
- Ensures atomic database operations

### 3. **Result Pattern**
Type-safe error handling without exceptions
```csharp
Result<User> result = await _authService.RegisterAsync(...);
if (result.IsSuccess) {
    // Use result.Data
} else {
    // Handle result.Error or result.Errors
}
```

### 4. **Dependency Injection**
All dependencies are injected via interfaces
- Easy to test with mocks
- Loose coupling between components
- Configured in `Program.cs`

### 5. **Middleware Pipeline**
Global concerns handled consistently
- `GlobalExceptionMiddleware` - Catches unhandled exceptions
- `RequestLoggingMiddleware` - Logs all requests

## 📊 Data Flow

```
Request → Controller/Page 
    → Service (IAuthService)
        → Repository (IUserRepository via UnitOfWork)
            → Database
        ← Result<T>
    ← Response
```

## 🔐 Authentication Flow

1. **Registration**:
   - Validate input → Check if user exists → Hash password → Save to DB
   
2. **Login**:
   - Validate input → Find user → Verify password → Generate JWT token

## 🧪 Benefits of This Architecture

✅ **Testable** - Easy to mock interfaces for unit testing
✅ **Maintainable** - Clear separation of concerns
✅ **Scalable** - Easy to add new features without breaking existing code
✅ **Flexible** - Can swap implementations (e.g., change database)
✅ **Type-Safe** - Result pattern prevents runtime errors
✅ **Centralized Error Handling** - Global middleware catches all errors
✅ **Validated** - Input validation at service layer

## 🚀 Adding New Features

### Adding a New Entity

1. Create model in `Models/`
2. Create repository interface in `Core/Interfaces/`
3. Implement repository in `Infrastructure/Repositories/`
4. Add to `UnitOfWork`
5. Create service interface in `Core/Interfaces/`
6. Implement service in `Application/Services/`
7. Create controller/pages

### Example: Adding Posts

```csharp
// 1. Model
public class Post { ... }

// 2. Interface
public interface IPostRepository : IRepository<Post> { ... }

// 3. Implementation
public class PostRepository : Repository<Post>, IPostRepository { ... }

// 4. Add to UnitOfWork
public interface IUnitOfWork {
    IPostRepository Posts { get; }
}

// 5. Service
public interface IPostService {
    Task<Result<Post>> CreatePostAsync(...);
}

// 6. Use in controller
public class PostsController {
    private readonly IPostService _postService;
    ...
}
```

## 📝 Configuration

- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development overrides
- JWT settings configured in Jwt section

## 🔧 Running the Application

```bash
# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Run the app
dotnet run
```

Access:
- API: `https://localhost:5001/api`
- Swagger: `https://localhost:5001/openapi`
- Web UI: `https://localhost:5001/`

## 👥 Best Practices Implemented

1. **Interface Segregation** - Small, focused interfaces
2. **Single Responsibility** - Each class has one job
3. **Dependency Inversion** - Depend on abstractions, not concretions
4. **Validation** - Input validated before processing
5. **Logging** - Structured logging throughout
6. **Error Handling** - Graceful error responses
7. **Security** - Password hashing, JWT tokens, HTTPS
