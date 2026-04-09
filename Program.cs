using Microsoft.EntityFrameworkCore;
using TruePal.Api.Data;
using TruePal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

//
// 1. Services (Dependency Injection)
//
builder.Services.AddControllers();

// OpenAPI / Swagger (built-in OpenAPI in .NET 8)
builder.Services.AddOpenApi();

builder.Services.AddScoped<AuthService>();

// DATABASE CONFIG
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=truepal.db");
});

var app = builder.Build();

//
// 2. Middleware pipeline
//
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//
// 3. Route mapping
//
app.MapControllers();

app.Run();