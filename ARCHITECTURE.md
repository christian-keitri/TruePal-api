# TruePal.Api - Architecture

## Overview

TruePal.Api follows **Clean Architecture** with two presentation layers: MVC (server-rendered views) and REST API (JSON endpoints). Both share the same service, repository, and domain layers.

```
Clients
  ├── Browser (MVC Views)       ─┐
  └── Mobile / SPA (REST API)   ─┤
                                  ▼
                          Controllers
                              │
                          Services (business logic, validation)
                              │
                          Repositories (data access via UnitOfWork)
                              │
                          SQLite Database
```

---

## Project Structure

```
TruePal.Api/
├── Controllers/
│   ├── Base/
│   │   └── BaseController.cs         # MVC base: TempData helpers, logging
│   ├── HomeController.cs             # MVC: Landing page, trending posts
│   ├── AuthController.cs             # MVC: Login, Register, Logout views
│   ├── DashboardController.cs        # MVC: User dashboard view
│   ├── ProfileController.cs          # MVC: User profile view
│   ├── ApiAuthController.cs          # API: POST /api/auth/register, /login
│   └── ApiPostsController.cs         # API: CRUD /api/posts/*
│
├── Application/
│   └── Services/
│       ├── AuthService.cs            # Registration, login, JWT generation
│       └── PostService.cs            # Post CRUD, validation, ownership
│
├── Core/
│   ├── Common/
│   │   └── Result.cs                 # Result<T>, ErrorCodes
│   ├── Interfaces/
│   │   ├── IRepository.cs            # Generic repository contract
│   │   ├── IUserRepository.cs        # User-specific queries
│   │   ├── IPostRepository.cs        # Post-specific queries
│   │   ├── IUnitOfWork.cs            # Transaction coordinator
│   │   ├── IAuthService.cs           # Auth service contract
│   │   └── IPostService.cs           # Post service contract
│   ├── Configuration/
│   │   └── JwtSettings.cs            # JWT config model
│   └── Validators/
│       └── ValidationHelper.cs       # Email, registration, login validation
│
├── Infrastructure/
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs   # Catches unhandled exceptions
│   │   └── RequestLoggingMiddleware.cs    # Logs request timing
│   ├── Repositories/
│   │   ├── Repository.cs             # Generic base implementation
│   │   ├── UserRepository.cs
│   │   └── PostRepository.cs
│   └── UnitOfWork.cs                 # Coordinates repositories + SaveChanges
│
├── Models/
│   ├── User.cs                       # Id, Username, Email, PasswordHash, CreatedAt
│   └── Post.cs                       # Id, Content, Location, ImageUrl, counts, UserId
│
├── DTOs/
│   ├── RegisterRequest.cs            # Registration input
│   ├── LoginRequest.cs               # Login input
│   ├── PostRequest.cs                # CreatePostDto, UpdatePostDto (with annotations)
│   └── PostResponse.cs               # PostResponse, PostUserResponse (with factories)
│
├── Data/
│   └── AppDbContext.cs               # EF Core context: Users, Posts
│
├── Views/                            # MVC Razor views
│   ├── Auth/                         # Login, Register, ForgotPassword
│   ├── Dashboard/                    # User dashboard
│   ├── Profile/                      # User profile
│   ├── Home/                         # Landing page
│   └── Shared/                       # Layout, partials, components
│
├── wwwroot/
│   ├── css/
│   │   ├── theme.css                 # Global CSS variables and base styles
│   │   ├── components/               # cards.css, panels.css, overlays.css
│   │   └── pages/                    # home.css, dashboard.css
│   ├── js/
│   │   ├── data/                     # locations.js, sample-posts.js
│   │   ├── services/                 # map-service.js, posts-service.js, ui-service.js
│   │   └── pages/                    # home.js, dashboard.js
│   └── images/
│
├── Migrations/                       # EF Core migrations
└── Program.cs                        # DI registration, middleware pipeline
```

---

## Design Patterns

### 1. Repository + Unit of Work

Repositories abstract data access. UnitOfWork coordinates them and ensures atomic commits.

```
IUnitOfWork
├── Users  (IUserRepository -> UserRepository)
├── Posts  (IPostRepository -> PostRepository)
└── CompleteAsync()  -> SaveChangesAsync()
```

Adding a new entity: create model, repository interface, implementation, and add to IUnitOfWork/UnitOfWork.

### 2. Result Pattern + Error Codes

Services return `Result<T>` instead of throwing exceptions. Each failure carries an `ErrorCode` that controllers map to HTTP status codes.

```
Service                              Controller
Result<Post>.Failure("...", NOT_FOUND)  -> 404 NotFound
Result<Post>.Failure("...", FORBIDDEN)  -> 403 Forbidden
Result<Post>.Failure(errors)            -> 400 BadRequest
Result<Post>.Success(post)              -> 200 OK
```

### 3. Two Controller Types

| | MVC Controllers | API Controllers |
|---|---|---|
| Base class | `BaseController` (extends `Controller`) | `ControllerBase` with `[ApiController]` |
| Returns | Views with ViewModels | JSON with Response DTOs |
| Error display | TempData messages | JSON `{ "error": "..." }` |
| Auth check | Cookie-based | JWT `[Authorize]` |
| CSRF | `[ValidateAntiForgeryToken]` | Not needed (JWT) |

### 4. Dependency Injection

All dependencies are registered as Scoped in `Program.cs`:

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
```

### 5. Middleware Pipeline

```
Request
  → GlobalExceptionMiddleware   (catches unhandled exceptions → 500 JSON)
  → RequestLoggingMiddleware    (logs method, path, duration, status)
  → HTTPS Redirection
  → Static Files
  → Authentication              (validates JWT)
  → Authorization               (enforces [Authorize])
  → Routing → Controller
```

---

## Data Flow

### API Request

```
POST /api/posts
  → ApiPostsController.CreatePost()
    → try-catch wrapper
      → _postService.CreatePostAsync(userId, content, location, imageUrl)
        → ValidatePostInput() → errors? → Result.Failure(errors)
        → _unitOfWork.Users.GetByIdAsync(userId) → null? → Result.Failure(NOT_FOUND)
        → _unitOfWork.Posts.AddAsync(post)
        → _unitOfWork.CompleteAsync()
        → Result.Success(post)
      → PostResponse.FromPost(post)
    → 201 Created + Location header
```

### MVC Request

```
GET /Dashboard
  → DashboardController.Index()
    → Check AuthToken cookie
    → _postService.GetRecentPostsAsync(10)
    → Build DashboardViewModel
    → return View(model)
```

---

## Database Schema

**Users**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, auto-increment |
| Username | TEXT | Required |
| Email | TEXT | Required |
| PasswordHash | TEXT | Required |
| CreatedAt | TEXT (DateTime) | Required |

**Posts**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, auto-increment |
| Content | TEXT | Required, max 500 |
| Location | TEXT | Optional, max 200 |
| ImageUrl | TEXT | Optional, max 500 |
| LikesCount | INTEGER | Default 0 |
| CommentsCount | INTEGER | Default 0 |
| ViewsCount | INTEGER | Default 0 |
| CreatedAt | TEXT (DateTime) | Required |
| UpdatedAt | TEXT (DateTime) | Nullable |
| UserId | INTEGER | FK -> Users.Id, cascade delete |

---

## API Endpoints

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login, returns JWT |

### Posts
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/posts/{id}` | No | Get post by ID |
| GET | `/api/posts/recent?count=10` | No | Recent posts feed |
| GET | `/api/posts/trending?count=10` | No | Trending posts |
| GET | `/api/posts/user/{userId}` | No | Posts by user |
| POST | `/api/posts` | JWT | Create post |
| PUT | `/api/posts/{id}` | JWT | Update post (owner only) |
| DELETE | `/api/posts/{id}` | JWT | Delete post (owner only) |
| POST | `/api/posts/{id}/views` | No | Increment view count |

### MVC Routes
| Route | Controller | Description |
|-------|-----------|-------------|
| `/` | Home | Landing page |
| `/Auth/Login` | Auth | Login form |
| `/Auth/Register` | Auth | Registration form |
| `/Dashboard` | Dashboard | User dashboard |
| `/Profile` | Profile | User profile |

---

## Adding a New Feature

Follow this sequence:

1. **Model** in `Models/` with data annotations
2. **Repository interface** extending `IRepository<T>` in `Core/Interfaces/`
3. **Repository implementation** extending `Repository<T>` in `Infrastructure/Repositories/`
4. **Add to UnitOfWork** (interface + implementation)
5. **Service interface** in `Core/Interfaces/` returning `Result<T>`
6. **Service implementation** in `Application/Services/` with validation + error codes
7. **Request DTOs** in `DTOs/` with `[Required]`, `[StringLength]`
8. **Response DTOs** in `DTOs/` with `FromModel()` factory
9. **Controller** (API or MVC) with proper error handling
10. **Register in Program.cs** as Scoped
11. **EF Migration** (`dotnet ef migrations add ...`)
12. **Tests** in `TruePal.Api.Tests/` matching source structure

See [CODING_STANDARDS.md](CODING_STANDARDS.md) for detailed rules on each step.

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 10 |
| Language | C# 13 |
| Database | SQLite via EF Core 10 |
| Auth | JWT Bearer tokens + BCrypt |
| Frontend | Razor Views + Bootstrap 5 + Bootstrap Icons |
| JavaScript | ES6 modules (vanilla) |
| Testing | xUnit + FluentAssertions + In-memory SQLite |
| API Docs | OpenAPI / Swagger |

---

**Last Updated:** April 15, 2026
