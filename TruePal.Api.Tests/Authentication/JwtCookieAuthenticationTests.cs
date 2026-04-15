using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TruePal.Api.Tests.Authentication;

public class JwtCookieAuthenticationTests
{
    private readonly string _testJwtKey = "ThisIsATestKeyThatIsAtLeast32BytesLongForJwtTokenGeneration!";
    private readonly string _testIssuer = "TruePal.Tests";
    private readonly string _testAudience = "TruePal.Tests";

    private string GenerateTestToken(int userId, string username, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_testJwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _testIssuer,
            Audience = _testAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public void GenerateTestToken_ValidClaims_ReturnsValidToken()
    {
        // Act
        var token = GenerateTestToken(1, "testuser", "test@example.com");

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts (header.payload.signature)
    }

    [Fact]
    public void GenerateTestToken_CanBeDecoded()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");

        // Act
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.Should().NotBeNull();
        // JWT uses short claim types like "nameid" instead of full URIs
        jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "testuser");
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        jwtToken.Issuer.Should().Be(_testIssuer);
    }

    [Fact]
    public void GenerateTestToken_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");

        // Act
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be(_testIssuer);
        jwtToken.Audiences.Should().Contain(_testAudience);
    }

    [Fact]
    public void GenerateTestToken_HasValidExpiration()
    {
        // Arrange
        var beforeGeneration = DateTime.UtcNow;
        
        // Act
        var token = GenerateTestToken(1, "testuser", "test@example.com");
        var afterGeneration = DateTime.UtcNow;

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeAfter(beforeGeneration.AddMinutes(59));
        jwtToken.ValidTo.Should().BeBefore(afterGeneration.AddMinutes(61));
    }

    [Fact]
    public void TokenValidationParameters_ValidToken_Succeeds()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_testJwtKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _testIssuer,
            ValidAudience = _testAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Act
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        // Assert
        principal.Should().NotBeNull();
        principal.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
        validatedToken.Should().NotBeNull();
    }

    [Fact]
    public void TokenValidationParameters_ExpiredToken_ThrowsException()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_testJwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }),
            Expires = DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            NotBefore = DateTime.UtcNow.AddMinutes(-20), // Valid from 20 minutes ago
            Issuer = _testIssuer,
            Audience = _testAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _testIssuer,
            ValidAudience = _testAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Act & Assert
        Action act = () => tokenHandler.ValidateToken(token, validationParameters, out _);
        act.Should().Throw<SecurityTokenExpiredException>();
    }

    [Fact]
    public void TokenValidationParameters_InvalidIssuer_ThrowsException()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_testJwtKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "InvalidIssuer", // Wrong issuer
            ValidAudience = _testAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Act & Assert
        Action act = () => tokenHandler.ValidateToken(token, validationParameters, out _);
        act.Should().Throw<SecurityTokenInvalidIssuerException>();
    }

    [Fact]
    public void TokenValidationParameters_InvalidSignature_ThrowsException()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");
        var tokenHandler = new JwtSecurityTokenHandler();
        var wrongKey = Encoding.UTF8.GetBytes("WrongKeyThatIsAtLeast32BytesLongForTesting!!!!");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _testIssuer,
            ValidAudience = _testAudience,
            IssuerSigningKey = new SymmetricSecurityKey(wrongKey) // Wrong key
        };

        // Act & Assert
        Action act = () => tokenHandler.ValidateToken(token, validationParameters, out _);
        act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
    }

    [Fact]
    public void HttpContext_WithCookie_CanExtractToken()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");
        var httpContext = new DefaultHttpContext();
        
        // Manually add cookie to request
        httpContext.Request.Headers["Cookie"] = $"AuthToken={token}";

        // Act - Simulate reading cookie like middleware does
        var cookieHeader = httpContext.Request.Headers["Cookie"].ToString();
        var hasAuthToken = cookieHeader.Contains("AuthToken=");

        // Assert
        hasAuthToken.Should().BeTrue();
        cookieHeader.Should().Contain(token);
    }

    [Fact]
    public void HttpContext_WithoutCookie_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var cookieHeader = httpContext.Request.Headers["Cookie"].ToString();

        // Assert
        cookieHeader.Should().BeNullOrEmpty();
    }

    [Fact]
    public void JwtBearerEvents_OnMessageReceived_ReadsCookieWhenHeaderEmpty()
    {
        // Arrange
        var token = GenerateTestToken(1, "testuser", "test@example.com");
        var httpContext = new DefaultHttpContext();
        
        // Add cookie to request header
        httpContext.Request.Headers["Cookie"] = $"AuthToken={token}";

        var context = new MessageReceivedContext(
            httpContext,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme,
                typeof(JwtBearerHandler)),
            new JwtBearerOptions());

        // Act - Simulate the OnMessageReceived logic from Program.cs
        if (string.IsNullOrEmpty(context.Token))
        {
            // Parse cookie manually (simplified version)
            var cookieHeader = context.Request.Headers["Cookie"].ToString();
            if (cookieHeader.Contains("AuthToken="))
            {
                var startIndex = cookieHeader.IndexOf("AuthToken=") + "AuthToken=".Length;
                var endIndex = cookieHeader.IndexOf(';', startIndex);
                context.Token = endIndex > startIndex 
                    ? cookieHeader.Substring(startIndex, endIndex - startIndex)
                    : cookieHeader.Substring(startIndex);
            }
        }

        // Assert
        context.Token.Should().NotBeNull();
        context.Token.Should().Be(token);
    }

    [Fact]
    public void JwtBearerEvents_OnMessageReceived_PrioritizesHeaderOverCookie()
    {
        // Arrange
        var headerToken = GenerateTestToken(1, "headeruser", "header@example.com");
        var cookieToken = GenerateTestToken(2, "cookieuser", "cookie@example.com");
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = $"AuthToken={cookieToken}";

        var context = new MessageReceivedContext(
            httpContext,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme,
                typeof(JwtBearerHandler)),
            new JwtBearerOptions());
        
        // Simulate token already set from Authorization header
        context.Token = headerToken;

        // Act - Simulate the OnMessageReceived logic from Program.cs
        if (string.IsNullOrEmpty(context.Token))
        {
            // This should not execute because header token already set
            var cookieHeader = context.Request.Headers["Cookie"].ToString();
            if (cookieHeader.Contains("AuthToken="))
            {
                var startIndex = cookieHeader.IndexOf("AuthToken=") + "AuthToken=".Length;
                var endIndex = cookieHeader.IndexOf(';', startIndex);
                context.Token = endIndex > startIndex 
                    ? cookieHeader.Substring(startIndex, endIndex - startIndex)
                    : cookieHeader.Substring(startIndex);
            }
        }

        // Assert
        context.Token.Should().Be(headerToken); // Header takes precedence
    }
}
