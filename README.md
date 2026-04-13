# TruePal.Api

A scalable ASP.NET Core web application using **MVC architecture** with Clean Architecture principles.

## 🚀 Quick Start

```bash
# Run the application
dotnet run

# Open in browser
# http://localhost:5000
```

## 📚 Documentation

### Getting Started
- **[QUICKSTART.md](QUICKSTART.md)** - Run the app and test API endpoints
- **[CODING_STANDARDS.md](CODING_STANDARDS.md)** ⭐ **REQUIRED READING** - Mandatory coding patterns and rules
- **[THEME_GUIDE.md](THEME_GUIDE.md)** - CSS variables and design system

### Architecture Guides
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Overall architecture and design patterns
- **[MVC_ARCHITECTURE.md](MVC_ARCHITECTURE.md)** - MVC folder structure and best practices
- **[CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md)** - CSS organization and component system

### Implementation Details
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - What changes were made and why
- **[MVC_SUMMARY.md](MVC_SUMMARY.md)** - Summary of MVC migration
- **[MVC_MIGRATION.md](MVC_MIGRATION.md)** - Complete migration guide from Razor Pages to MVC

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────┐
│   MVC Controllers & Views           │ ← Presentation Layer
├─────────────────────────────────────┤
│    Services (IAuthService)          │ ← Business Logic Layer
├─────────────────────────────────────┤
│    Repositories (IUnitOfWork)       │ ← Data Access Layer
├─────────────────────────────────────┤
│    Database (SQLite)                │ ← Data Layer
└─────────────────────────────────────┘
```

## 🎯 Features

- ✅ **MVC Architecture** - Model-View-Controller pattern
- ✅ **Clean Architecture** - Separation of concerns with Core/Application/Infrastructure layers
- ✅ **Repository Pattern** - Abstracted data access
- ✅ **Unit of Work** - Transaction management
- ✅ **Result Pattern** - Type-safe error handling
- ✅ **JWT Authentication** - Secure token-based auth
- ✅ **Cookie-based Sessions** - HttpOnly secure cookies
- ✅ **Input Validation** - Data annotations with client & server validation
- ✅ **Global Error Handling** - Middleware-based exception handling
- ✅ **Request Logging** - All requests logged
- ✅ **Component-based CSS** - Scalable stylesheet architecture
- ✅ **REST API** - JSON endpoints for mobile/SPA clients

## 📁 Project Structure

```
TruePal.Api/
├── Controllers/              # MVC & API Controllers
│   ├── Base/                # BaseController with helpers
│   ├── Auth/Dashboard/Profile/Home  # MVC Controllers
│   └── ApiAuth/ApiPosts/ApiUsers    # REST API Controllers
├── Views/                    # Razor Views (.cshtml)
│   ├── Auth/Dashboard/Profile/Home  # Feature-based views
│   └── Shared/              # Layouts & components
├── Core/                     # Domain layer
│   ├── Common/              # Result pattern
│   ├── Interfaces/          # Service & repository contracts
│   └── Validators/          # Business validation
├── Application/              # Business logic
│   └── Services/            # Service implementations
├── Infrastructure/           # External concerns
│   ├── Middleware/          # Global middleware
│   └── Repositories/        # Data access implementations
├── Models/                   # Domain entities
├── DTOs/                     # Data transfer objects
├── Data/                     # DbContext
└── wwwroot/                  # Static files
    └── css/
        ├── theme.css        # Global theme
        ├── components/      # Reusable component styles
        └── pages/           # Page-specific styles
```

## 🛣️ Routes

### Web Routes (MVC)
- `/` - Home page with interactive map
- `/Auth/Login` - Login
- `/Auth/Register` - Registration
- `/Dashboard/Index` - User dashboard
- `/Profile/Index` - User profile

### API Routes (REST)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login (returns JWT)
- `GET /api/posts` - Get posts
- `POST /api/posts` - Create post

## 🔐 Security Features

- ✅ Password hashing with BCrypt
- ✅ JWT token validation with signature & expiration
- ✅ HttpOnly, Secure, SameSite cookies
- ✅ XSS protection with HTML escaping
- ✅ CSRF protection with AntiForgeryToken
- ✅ SQL injection prevention (EF Core parameterization)
- ✅ Global exception handling (no error details leaked)

## 🎨 Design System

- **Primary Color**: Smokey Yellow (#c9a961)
- **UI Framework**: Bootstrap 5.3.0
- **Icons**: Bootstrap Icons
- **Maps**: MapLibre GL JS (free, no API key)
- **CSS Architecture**: Component-based with theme variables

See [THEME_GUIDE.md](THEME_GUIDE.md) for complete design system documentation.

## 🧪 Testing

### Test API with curl

**Register:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john",
    "email": "john@example.com",
    "password": "securepass123"
  }'
```

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "securepass123"
  }'
```

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C# 12
- **Database**: SQLite (EF Core)
- **Authentication**: JWT + Cookies
- **Frontend**: Razor Views, Bootstrap 5, Vanilla JS
- **Mapping**: MapLibre GL JS
- **Validation**: Data Annotations + jQuery Validation

## 📦 NuGet Packages

- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.AspNetCore.Authentication.JwtBearer
- BCrypt.Net-Next
- System.IdentityModel.Tokens.Jwt

## 🚦 Development

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Run with hot reload
dotnet watch run

# Database migrations
dotnet ef migrations add MigrationName
dotnet ef database update
```

## 📖 Learn More

- [ARCHITECTURE.md](ARCHITECTURE.md) - Deep dive into architecture patterns
- [MVC_MIGRATION.md](MVC_MIGRATION.md) - Learn about the MVC migration
- [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md) - CSS organization best practices

## 📝 License

Private project - All rights reserved © 2026 TruePal

---

**Built with ❤️ using ASP.NET Core MVC**
