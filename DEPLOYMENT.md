# TruePal.Api - Deployment Guide

## Environments

| Environment | Purpose | Database | Config |
|-------------|---------|----------|--------|
| Development | Local dev | SQLite (truepal.db) | appsettings.Development.json |
| Staging | Pre-production testing | PostgreSQL / SQL Server | appsettings.Staging.json |
| Production | Live users | PostgreSQL / SQL Server | appsettings.Production.json + env vars |

---

## Prerequisites

- .NET 10 SDK (build machine)
- .NET 10 Runtime (production server)
- A production database (PostgreSQL recommended)
- HTTPS certificate (Let's Encrypt or purchased)
- A reverse proxy (Nginx, Caddy, or cloud load balancer)

---

## Configuration

### Secrets Management

**The `Jwt:Key` in appsettings.json is a placeholder.** Never deploy the default key.

For production, use environment variables or a secrets manager:

```bash
# Environment variables (highest priority, overrides appsettings)
export Jwt__Key="your-production-secret-key-minimum-32-chars"
export Jwt__Issuer="TruePal.Api"
export Jwt__Audience="TruePal.Client"
export ConnectionStrings__DefaultConnection="Host=db.example.com;Database=truepal;Username=app;Password=..."
```

For local development, use .NET User Secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-dev-secret-key-minimum-32-chars"
```

**Never commit secrets to git.** Add to `.gitignore`:
```
appsettings.Production.json
appsettings.Staging.json
```

### appsettings.Production.json (template)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "Jwt": {
    "Key": "SET_VIA_ENVIRONMENT_VARIABLE",
    "Issuer": "TruePal.Api",
    "Audience": "TruePal.Client",
    "ExpireMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "SET_VIA_ENVIRONMENT_VARIABLE"
  }
}
```

---

## Build & Publish

### Build for Production

```bash
# Release build
dotnet publish -c Release -o ./publish

# The output is in ./publish/
# Contains: TruePal.Api.dll, wwwroot/, appsettings.json, etc.
```

### Run the Published App

```bash
cd publish
dotnet TruePal.Api.dll --urls "http://0.0.0.0:5000"
```

In production, always run behind a reverse proxy that handles HTTPS.

---

## Database Migration (Production)

### Before First Deploy

```bash
# Generate migration bundle (portable, no SDK needed on server)
dotnet ef migrations bundle -o efbundle --self-contained

# Run on production server
./efbundle --connection "your-production-connection-string"
```

### Subsequent Deploys

```bash
# Create migration for schema changes
dotnet ef migrations add DescriptiveName

# Test locally
dotnet ef database update

# Generate bundle and deploy
dotnet ef migrations bundle -o efbundle --self-contained
```

**Rule:** Always test migrations on a staging database before production. Never run `dotnet ef database update` directly on production.

---

## Reverse Proxy Setup

### Nginx

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### Caddy (simpler alternative)

```
yourdomain.com {
    reverse_proxy localhost:5000
}
```

Caddy auto-provisions HTTPS certificates.

---

## Process Management

### systemd (Linux)

Create `/etc/systemd/system/truepal.service`:

```ini
[Unit]
Description=TruePal API
After=network.target

[Service]
WorkingDirectory=/var/www/truepal
ExecStart=/usr/bin/dotnet /var/www/truepal/TruePal.Api.dll --urls "http://0.0.0.0:5000"
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=Jwt__Key=your-secret-key

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable truepal
sudo systemctl start truepal
sudo systemctl status truepal
```

---

## Cloud Deployment Options

### Azure App Service
```bash
az webapp create --name truepal-api --resource-group mygroup --plan myplan --runtime "DOTNET:10.0"
az webapp config appsettings set --name truepal-api --resource-group mygroup --settings Jwt__Key="secret"
dotnet publish -c Release
az webapp deploy --name truepal-api --src-path ./publish
```

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TruePal.Api.dll", "--urls", "http://0.0.0.0:5000"]
```

```bash
docker build -t truepal-api .
docker run -d -p 5000:5000 \
  -e Jwt__Key="your-secret" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  truepal-api
```

---

## Pre-Deployment Checklist

- [ ] JWT secret key changed from default (minimum 32 characters)
- [ ] Database connection string configured
- [ ] Migrations applied to production database
- [ ] HTTPS enabled (via reverse proxy or cloud provider)
- [ ] `AllowedHosts` restricted to your domain
- [ ] Log level set to `Warning` (not `Information`)
- [ ] Static files served efficiently (CDN or cache headers)
- [ ] `ASPNETCORE_ENVIRONMENT` set to `Production`
- [ ] Health check endpoint working
- [ ] Backup strategy for database
- [ ] Monitoring/alerting configured

## Post-Deployment Verification

```bash
# Health check
curl https://yourdomain.com/api/auth/login -X POST \
  -H "Content-Type: application/json" \
  -d '{"email":"test","password":"test"}' \
  -w "\n%{http_code}"
# Should return 401 (not 500)

# Check logs
journalctl -u truepal -f
```

---

**Last Updated:** April 15, 2026
