# TruePal.Api - Database Guide

## Current Setup

| Item | Value |
|------|-------|
| Database | SQLite |
| ORM | Entity Framework Core 10 |
| File | `truepal.db` (project root) |
| Context | `Data/AppDbContext.cs` |

---

## Schema

### Users
```sql
CREATE TABLE Users (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Username    TEXT    NOT NULL,
    Email       TEXT    NOT NULL,
    PasswordHash TEXT   NOT NULL,
    CreatedAt   TEXT    NOT NULL  -- DateTime as ISO 8601
);
```

### Posts
```sql
CREATE TABLE Posts (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    Content       TEXT    NOT NULL,   -- max 500 chars
    Location      TEXT,               -- max 200 chars
    ImageUrl      TEXT,               -- max 500 chars
    LikesCount    INTEGER NOT NULL DEFAULT 0,
    CommentsCount INTEGER NOT NULL DEFAULT 0,
    ViewsCount    INTEGER NOT NULL DEFAULT 0,
    CreatedAt     TEXT    NOT NULL,
    UpdatedAt     TEXT,
    UserId        INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Posts_UserId ON Posts(UserId);
```

---

## Migrations

### Creating a Migration

When you change a model or add a new entity:

```bash
# 1. Make your changes to Models/ and AppDbContext

# 2. Create the migration
dotnet ef migrations add DescriptiveName

# Examples:
dotnet ef migrations add AddCommentsTable
dotnet ef migrations add AddBioToUser
dotnet ef migrations add AddLikesUniqueIndex

# 3. Review the generated migration in Migrations/
# Verify it does what you expect

# 4. Apply to local database
dotnet ef database update
```

### Rolling Back

```bash
# Revert to a specific migration
dotnet ef database update PreviousMigrationName

# Remove the last migration (if not applied)
dotnet ef migrations remove
```

### Production Migrations

Never run `dotnet ef database update` directly on production. Use migration bundles:

```bash
# Generate a self-contained migration bundle
dotnet ef migrations bundle -o efbundle --self-contained

# Run on production
./efbundle --connection "your-production-connection-string"
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for full production migration strategy.

---

## Adding a New Entity

Follow this sequence:

### 1. Create the Model

```csharp
// Models/Comment.cs
public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int PostId { get; set; }

    [ForeignKey("PostId")]
    public virtual Post Post { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
```

### 2. Add DbSet to AppDbContext

```csharp
// Data/AppDbContext.cs
public DbSet<Comment> Comments { get; set; }
```

### 3. Create Migration

```bash
dotnet ef migrations add AddCommentsTable
dotnet ef database update
```

### 4. Create Repository, Service, Controller

Follow [CODING_STANDARDS.md](CODING_STANDARDS.md) and [ARCHITECTURE.md](ARCHITECTURE.md).

---

## Indexing Guidelines

Add indexes for:
- **Foreign keys** - EF Core creates these automatically for navigation properties
- **Columns used in WHERE clauses** - e.g., Email on Users for login lookup
- **Columns used in ORDER BY** - e.g., CreatedAt on Posts for feeds
- **Unique constraints** - e.g., Email on Users (currently not enforced at DB level)

```csharp
// In AppDbContext.OnModelCreating or via attributes
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();

modelBuilder.Entity<Post>()
    .HasIndex(p => p.CreatedAt);
```

**Rule:** Add indexes when you observe slow queries, not preemptively. Profile first.

---

## Scaling Path: SQLite to PostgreSQL

SQLite works for development and small deployments. When you need concurrent writes, advanced queries, or multiple app instances, migrate to PostgreSQL.

### Step 1: Add PostgreSQL Package

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### Step 2: Update Program.cs

```csharp
// Replace SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Step 3: Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=truepal;Username=app;Password=secret"
  }
}
```

### Step 4: Re-generate Migrations

```bash
# Remove SQLite migrations
rm -rf Migrations/

# Create fresh PostgreSQL migration
dotnet ef migrations add InitialPostgres
dotnet ef database update
```

### What Changes

| Feature | SQLite | PostgreSQL |
|---------|--------|-----------|
| Concurrent writes | Single writer | Full MVCC |
| Full-text search | Limited | Built-in `tsvector` |
| JSON queries | No | `jsonb` column type |
| Max connections | 1 write | Pooled (100+) |
| Scaling | Single file | Replicas, partitioning |
| Backups | Copy file | `pg_dump`, continuous archiving |

---

## Backup Strategy

### Development (SQLite)

```bash
# Just copy the file
cp truepal.db truepal.db.backup
```

### Production (PostgreSQL)

```bash
# Full backup
pg_dump -U app -d truepal > backup_$(date +%Y%m%d).sql

# Restore
psql -U app -d truepal < backup_20260415.sql
```

Automate with cron:
```bash
0 2 * * * pg_dump -U app -d truepal | gzip > /backups/truepal_$(date +\%Y\%m\%d).sql.gz
```

---

## Data Seeding

For development, seed test data:

```csharp
// Data/SeedData.cs
public static class SeedData
{
    public static async Task Initialize(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return; // already seeded

        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var posts = Enumerable.Range(1, 10).Select(i => new Post
        {
            UserId = user.Id,
            Content = $"Sample post #{i}",
            CreatedAt = DateTime.UtcNow.AddHours(-i)
        });
        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
    }
}
```

Call from Program.cs (development only):
```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.Initialize(context);
}
```

---

**Last Updated:** April 15, 2026
