using Microsoft.EntityFrameworkCore;
using TruePal.Api.Data;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Application.Services;
using TruePal.Api.Infrastructure;
using TruePal.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//
// 1. Services (Dependency Injection)
//
builder.Services.AddControllersWithViews();

// OpenAPI / Swagger (built-in OpenAPI in .NET 8)
builder.Services.AddOpenApi();

// Dependency Injection - Repositories and Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();

// DATABASE CONFIG
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=truepal.db");
});

var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
    };

    // Configure JWT to read from cookies for API calls from authenticated MVC pages
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for token in Authorization header first (for API clients)
            if (string.IsNullOrEmpty(context.Token))
            {
                // If not in header, check the AuthToken cookie (for MVC authenticated requests)
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

//
// 2. Middleware pipeline
//
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Global error handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

//
// 3. Route mapping
//
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
