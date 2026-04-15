# TruePal.Api

A social platform API and web application built with ASP.NET Core 10, following Clean Architecture principles.

## Quick Start

```bash
# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Run the application
dotnet run

# Run tests
cd TruePal.Api.Tests && dotnet test
```

**Access:**
- Web UI: `https://localhost:5001/`
- API: `https://localhost:5001/api/`
- Swagger: `https://localhost:5001/openapi`

---

## Architecture

Clean Architecture with two presentation layers sharing the same backend:

```
Browser (MVC Views)  ──┐
Mobile / SPA (API)   ──┤
                        ▼
                   Controllers
                        │
                   Services (business logic)
                        │
                   Repositories (data access via UnitOfWork)
                        │
                   SQLite Database
```

**Key patterns:** Repository + Unit of Work, Result Pattern with Error Codes, Dependency Injection, Response DTOs.

See [ARCHITECTURE.md](ARCHITECTURE.md) for full details.

---

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register (returns user) |
| POST | `/api/auth/login` | Login (returns JWT token) |

### Posts
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/posts/{id}` | - | Get post by ID |
| GET | `/api/posts/recent?count=10` | - | Recent posts feed |
| GET | `/api/posts/trending?count=10` | - | Trending posts |
| GET | `/api/posts/user/{userId}` | - | Posts by user |
| POST | `/api/posts` | JWT | Create post |
| PUT | `/api/posts/{id}` | JWT | Update post (owner only) |
| DELETE | `/api/posts/{id}` | JWT | Delete post (owner only) |
| POST | `/api/posts/{id}/views` | - | Track view |

### Web Pages
| Route | Description |
|-------|-------------|
| `/` | Landing page with trending posts |
| `/Auth/Login` | Login form |
| `/Auth/Register` | Registration form |
| `/Dashboard` | User dashboard (authenticated) |
| `/Profile` | User profile (authenticated) |

---

## Project Structure

```
TruePal.Api/
├── Controllers/          # MVC + API controllers
├── Application/Services/ # Business logic
├── Core/                 # Interfaces, Result<T>, validators
├── Infrastructure/       # Repositories, middleware, UnitOfWork
├── Models/               # Domain entities (User, Post)
├── DTOs/                 # Request/response objects
├── Data/                 # EF Core DbContext
├── Views/                # Razor views
├── wwwroot/              # Static assets (CSS, JS, images)
├── Migrations/           # Database migrations
└── TruePal.Api.Tests/    # Unit tests
```

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 10 |
| Database | SQLite + EF Core 10 |
| Auth | JWT Bearer + BCrypt |
| Frontend | Razor + Bootstrap 5 |
| Testing | xUnit + FluentAssertions |

---

## Documentation

| Document | Purpose |
|----------|---------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture, data flow, patterns |
| [CODING_STANDARDS.md](CODING_STANDARDS.md) | 57 mandatory development rules |
| [TESTING_GUIDE.md](TESTING_GUIDE.md) | Testing patterns, FluentAssertions, coverage |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Development workflow, PR checklist |
| [QUICKSTART.md](QUICKSTART.md) | Setup guide with API examples |
| [CSS_ARCHITECTURE.md](CSS_ARCHITECTURE.md) | CSS organization and components |
| [THEME_GUIDE.md](THEME_GUIDE.md) | Design tokens and CSS variables |
| [UI_UX_STANDARDS.md](UI_UX_STANDARDS.md) | UI/UX design rules |
| [JS_ARCHITECTURE.md](JS_ARCHITECTURE.md) | JavaScript module organization |
| [COLOR_PALETTE.md](COLOR_PALETTE.md) | Color system reference |

---

## Security

- Passwords hashed with BCrypt (never stored in plain text)
- JWT tokens with full claim validation (issuer, audience, lifetime, signing key)
- HttpOnly + Secure + SameSite=Strict cookies
- CSRF protection on MVC forms (`[ValidateAntiForgeryToken]`)
- Global exception middleware prevents internal details from leaking
- User input escaped in JavaScript rendering
- Ownership verification on all mutations (update/delete)

---

**Last Updated:** April 15, 2026
