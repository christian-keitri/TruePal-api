# Changelog

All notable changes to TruePal.Api are documented here.

Format follows [Keep a Changelog](https://keepachangelog.com/).

---

## [1.0.0] - 2026-04-15

### Added
- **Authentication:** User registration and login with JWT tokens
- **Posts API:** Full CRUD (create, read, update, delete)
- **Post feeds:** Recent posts and trending posts endpoints
- **View tracking:** Dedicated `POST /api/posts/{id}/views` endpoint
- **Ownership enforcement:** Update/delete restricted to post owner
- **Result pattern:** `Result<T>` with typed `ErrorCodes` (NOT_FOUND, FORBIDDEN, VALIDATION)
- **Response DTOs:** `PostResponse` with `FromPost()` factory (no domain model exposure)
- **Input validation:** Data annotations on DTOs + service-layer business validation
- **Error handling:** `MapError()` controller helper routes ErrorCode to HTTP status
- **Middleware:** GlobalExceptionMiddleware + RequestLoggingMiddleware
- **MVC web UI:** Landing page, login, register, dashboard, profile views
- **Testing:** 56 unit tests with xUnit + FluentAssertions + in-memory SQLite
- **Documentation:** CODING_STANDARDS (57 rules), ARCHITECTURE, API_REFERENCE, TESTING_GUIDE, and 13 other guides

### Architecture
- Clean Architecture: Controllers -> Services -> Repositories -> Database
- Unit of Work pattern for atomic database operations
- Generic `Repository<T>` base class with common CRUD operations
- Two controller types: MVC (`BaseController`) and API (`ControllerBase` + `[ApiController]`)
- Component-based CSS with theme variables (Goldenrod + Jet Black)
- ES6 modular JavaScript (services, pages, data layers)

### Security
- BCrypt password hashing
- JWT with full claim validation (issuer, audience, lifetime, signing key)
- HttpOnly + Secure + SameSite cookies for MVC auth
- CSRF protection on MVC forms
- Global exception middleware prevents internal detail leaks

---

## [Unreleased]

### Planned
- Likes and comments (see ROADMAP.md Phase 1)
- User follows and personalized feed
- User profile editing (bio, avatar)
- Notifications system

---

**Convention:** When adding entries, use these categories:
- **Added** - New features
- **Changed** - Changes to existing features
- **Fixed** - Bug fixes
- **Removed** - Removed features
- **Security** - Security-related changes
