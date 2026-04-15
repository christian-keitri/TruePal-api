# TruePal.Api - Quick Start Guide

## Prerequisites

- .NET 10 SDK
- A terminal / command line

## Setup

```bash
# 1. Restore dependencies
dotnet restore

# 2. Create / update the database
dotnet ef database update

# 3. Run the application
dotnet run
```

The app runs at `https://localhost:5001` (or `http://localhost:5000`).

---

## Try the API

### Register a User

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Password123!"
  }'
```

### Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!"
  }'
```

Save the returned JWT token.

### Create a Post

```bash
curl -X POST https://localhost:5001/api/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "content": "Hello TruePal!",
    "location": "Manila"
  }'
```

### Get Recent Posts

```bash
curl https://localhost:5001/api/posts/recent?count=5
```

### Get Trending Posts

```bash
curl https://localhost:5001/api/posts/trending?count=5
```

### Track a View

```bash
curl -X POST https://localhost:5001/api/posts/1/views
```

### Update a Post (owner only)

```bash
curl -X PUT https://localhost:5001/api/posts/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "content": "Updated content!",
    "location": "Cebu"
  }'
```

### Delete a Post (owner only)

```bash
curl -X DELETE https://localhost:5001/api/posts/1 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Web UI

| Page | URL | Description |
|------|-----|-------------|
| Home | `/` | Landing page with trending posts |
| Login | `/Auth/Login` | Login form |
| Register | `/Auth/Register` | Registration form |
| Dashboard | `/Dashboard` | User dashboard (requires login) |
| Profile | `/Profile` | User profile (requires login) |

---

## Running Tests

```bash
cd TruePal.Api.Tests
dotnet test
```

All tests must pass before committing.

---

## Error Response Format

All API errors return consistent JSON:

```json
// Single error
{ "error": "Post not found" }

// Multiple validation errors
{ "errors": ["Content is required", "Location too long"] }
```

HTTP status codes: 400 (validation), 401 (auth), 403 (forbidden), 404 (not found), 500 (server error).

---

## Next Steps

- Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand the system design
- Read [CODING_STANDARDS.md](CODING_STANDARDS.md) before writing code (57 rules)
- Read [TESTING_GUIDE.md](TESTING_GUIDE.md) for test patterns
- Read [CONTRIBUTING.md](CONTRIBUTING.md) for the development workflow

---

**Last Updated:** April 15, 2026
