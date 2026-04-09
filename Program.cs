using Microsoft.EntityFrameworkCore;
using TruePal.Api.Data;
using TruePal.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
app.UseAuthentication();
app.UseAuthorization();

//
// 3. Route mapping
//
app.MapControllers();

app.Run();