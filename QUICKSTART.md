# TruePal.Api - Quick Start Guide

## 🚀 Your Application is Now Scalable!

Your login and signup Razor pages are now connected to the database with a professional, enterprise-grade architecture.

## ✅ What's Working

- ✅ Login page at `/Login`
- ✅ Registration page at `/Register`
- ✅ API endpoints at `/api/auth/login` and `/api/auth/register`
- ✅ Database connection (SQLite)
- ✅ Input validation
- ✅ Error handling
- ✅ Request logging
- ✅ JWT authentication

## 🏃 Run the Application

```bash
dotnet run
```

Then open your browser to:
- **Home Page**: http://localhost:5000/
- **Login**: http://localhost:5000/Login
- **Register**: http://localhost:5000/Register
- **API Docs**: http://localhost:5000/openapi (in development mode)

## 📝 Test the API

### Register a User
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john",
    "email": "john@example.com",
    "password": "securepass123"
  }'
```

**Response (Success):**
```json
{
  "id": 1,
  "username": "john",
  "email": "john@example.com"
}
```

**Response (Validation Error):**
```json
{
  "errors": [
    "Username must be at least 3 characters",
    "Email is not valid",
    "Password must be at least 6 characters"
  ]
}
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "securepass123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

## 🏗️ Architecture Overview

Your app now has these layers:

```
┌─────────────────────────────────────┐
│     Controllers & Razor Pages       │ ← Presentation Layer
├─────────────────────────────────────┤
│    Services (IAuthService)          │ ← Business Logic Layer
├─────────────────────────────────────┤
│    Repositories (IUnitOfWork)       │ ← Data Access Layer
├─────────────────────────────────────┤
│    Database (SQLite)                │ ← Data Layer
└─────────────────────────────────────┘
```

## 📚 Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed architecture explanation
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - What changed and why

## 🎯 Key Features

### 1. Input Validation
All inputs are validated before processing:
- Username: 3-50 characters
- Email: Valid email format
- Password: Minimum 6 characters

### 2. Error Handling
Errors are handled gracefully with clear messages:
```json
// Multiple validation errors
{"errors": ["Error 1", "Error 2"]}

// Single error
{"error": "Specific error message"}
```

### 3. Logging
Every request is logged with:
- HTTP method and path
- Response time in milliseconds
- Status code
- Database queries

### 4. Security
- Passwords are hashed with BCrypt
- JWT tokens for authentication
- HTTPS redirection
- SQL injection prevention

## 🔧 Configuration

Edit `appsettings.json` to configure:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-change-this-in-production",
    "Issuer": "TruePal.Api",
    "Audience": "TruePal.Client",
    "ExpireMinutes": 60
  }
}
```

## 📊 Database

The app uses SQLite with Entity Framework Core. Database file: `truepal.db`

### Run Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### View Database
```bash
sqlite3 truepal.db
.tables
SELECT * FROM Users;
```

## 🧪 Testing

### Test Registration Flow
1. Go to http://localhost:5000/Register
2. Fill in the form:
   - Username: testuser
   - Email: test@example.com
   - Password: password123
   - Confirm Password: password123
3. Click "Register"
4. Should redirect to Login page

### Test Login Flow
1. Go to http://localhost:5000/Login
2. Enter credentials:
   - Email: test@example.com
   - Password: password123
3. Click "Login"
4. Should redirect to Home page with auth cookie set

### Test Validation
Try registering with:
- Username: "ab" (too short)
- Email: "notanemail" (invalid)
- Password: "123" (too short)

You should see all validation errors displayed.

## 🐛 Troubleshooting

### Database Errors
```bash
# Delete and recreate database
rm truepal.db
dotnet ef database update
```

### Port Already in Use
```bash
# Use a different port
dotnet run --urls "http://localhost:5001"
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

## 📈 Next Steps

1. **Add More Features**
   - User profiles
   - Posts/content
   - Friends/connections
   - Comments

2. **Improve Security**
   - Email verification
   - Password reset
   - Two-factor authentication
   - Rate limiting

3. **Add Testing**
   - Unit tests
   - Integration tests
   - End-to-end tests

4. **Deploy**
   - Docker containerization
   - Azure/AWS deployment
   - CI/CD pipeline

5. **Performance**
   - Redis caching
   - Database indexing
   - CDN for static files

## 💡 Tips

- Use `dotnet watch run` for auto-reload during development
- Check logs in the terminal for debugging
- Use browser DevTools to inspect network requests
- Read ARCHITECTURE.md to understand the design patterns

## 🎉 You're All Set!

Your application is now production-ready with:
- Clean architecture
- Proper error handling
- Input validation
- Security best practices
- Logging and monitoring
- Scalable design

Happy coding! 🚀
