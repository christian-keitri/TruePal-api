using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TruePal.Api.Controllers;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Models;
using TruePal.Api.ViewModels;
using FluentAssertions;

namespace TruePal.Api.Tests.Controllers;

public class ProfileControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPostService> _mockPostService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly IConfiguration _configuration;
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPostService = new Mock<IPostService>();
        _mockUserService = new Mock<IUserService>();
        _mockFileService = new Mock<IFileService>();
        _mockUserRepository = new Mock<IUserRepository>();

        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "ThisIsAVeryLongSecretKeyForJWTTokenGeneration123456"},
            {"JwtSettings:Issuer", "TruePalApp"},
            {"JwtSettings:Audience", "TruePalUsers"},
            {"JwtSettings:ExpiryInMinutes", "60"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _controller = new ProfileController(
            _mockUnitOfWork.Object,
            _mockPostService.Object,
            _mockUserService.Object,
            _mockFileService.Object,
            _configuration,
            NullLogger<ProfileController>.Instance
        );

        // Setup TempData for controller (needed for error/success messages)
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
    }

    private void SetupAuthenticatedUser(int userId)
    {
        // Generate a real JWT token
        var secretKey = _configuration["JwtSettings:SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Create HttpContext with mocked cookies
        var httpContext = new DefaultHttpContext();
        
        // Mock the cookies collection
        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c.ContainsKey("AuthToken")).Returns(true);
        mockCookies.Setup(c => c["AuthToken"]).Returns(tokenString);
        mockCookies.Setup(c => c.GetEnumerator()).Returns(
            new Dictionary<string, string> { { "AuthToken", tokenString } }.GetEnumerator()
        );

        // Create a custom request feature with our mocked cookies
        var requestFeature = new HttpRequestFeature
        {
            Headers = new HeaderDictionary()
        };
        httpContext.Features.Set<IHttpRequestFeature>(requestFeature);
        
        // Use a wrapper to override Request.Cookies
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
        mockRequest.SetupGet(r => r.HttpContext).Returns(httpContext);
        
        // Create a custom HttpContext that returns our mocked request
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(c => c.Features).Returns(httpContext.Features);
        mockHttpContext.Setup(c => c.Response).Returns(httpContext.Response);
        mockHttpContext.Setup(c => c.Items).Returns(httpContext.Items);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    #region Edit GET Tests

    [Fact]
    public async Task Edit_Get_NotAuthenticated_RedirectsToLogin()
    {
        // Arrange
        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c.ContainsKey("AuthToken")).Returns(false);
        
        var httpContext = new DefaultHttpContext();
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        // Act
        var result = await _controller.Edit() as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Login");
        result.ControllerName.Should().Be("Auth");
    }

    [Fact]
    public async Task Edit_Get_AuthenticatedUser_ReturnsViewWithModel()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Test bio",
            ProfilePictureUrl = "/uploads/pic.jpg"
        };

        SetupAuthenticatedUser(userId);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _controller.Edit() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<EditProfileViewModel>();
        var model = result.Model as EditProfileViewModel;
        model!.Username.Should().Be("testuser");
        model.Email.Should().Be("test@example.com");
        model.Bio.Should().Be("Test bio");
        model.CurrentProfilePictureUrl.Should().Be("/uploads/pic.jpg");
    }

    [Fact]
    public async Task Edit_Get_UserNotFound_RedirectsToLogin()
    {
        // Arrange
        var userId = 1;
        SetupAuthenticatedUser(userId);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Edit() as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Login");
        result.ControllerName.Should().Be("Auth");
    }

    #endregion

    #region Edit POST Tests

    [Fact]
    public async Task Edit_Post_NotAuthenticated_RedirectsToLogin()
    {
        // Arrange
        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c.ContainsKey("AuthToken")).Returns(false);
        
        var httpContext = new DefaultHttpContext();
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var model = new EditProfileViewModel();

        // Act
        var result = await _controller.Edit(model) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Login");
        result.ControllerName.Should().Be("Auth");
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsViewWithErrors()
    {
        // Arrange
        var userId = 1;
        SetupAuthenticatedUser(userId);

        var model = new EditProfileViewModel();
        _controller.ModelState.AddModelError("Bio", "Bio is too long");

        // Act
        var result = await _controller.Edit(model) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().Be(model);
        _controller.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Edit_Post_ValidModelWithoutPicture_UpdatesProfile()
    {
        // Arrange
        var userId = 1;
        SetupAuthenticatedUser(userId);

        var model = new EditProfileViewModel
        {
            Bio = "Updated bio",
            CurrentProfilePictureUrl = "/uploads/old.jpg"
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Updated bio",
            ProfilePictureUrl = "/uploads/old.jpg"
        };

        _mockUserService.Setup(s => s.UpdateProfileAsync(userId, "Updated bio", "/uploads/old.jpg"))
            .ReturnsAsync(Result<User>.Success(updatedUser));

        // Act
        var result = await _controller.Edit(model) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Index");
        result.ControllerName.Should().Be("Profile");
        _mockUserService.Verify(s => s.UpdateProfileAsync(userId, "Updated bio", "/uploads/old.jpg"), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_WithProfilePicture_UploadsAndUpdatesProfile()
    {
        // Arrange
        var userId = 1;
        SetupAuthenticatedUser(userId);

        var fileMock = new Mock<IFormFile>();
        var content = "fake image content";
        var fileName = "test.jpg";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var model = new EditProfileViewModel
        {
            Bio = "Updated bio",
            ProfilePicture = fileMock.Object,
            CurrentProfilePictureUrl = "/uploads/old.jpg"
        };

        var uploadedUrl = "/uploads/new.jpg";
        _mockFileService.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), fileName, "image/jpeg"))
            .ReturnsAsync(Result<string>.Success(uploadedUrl));

        _mockFileService.Setup(s => s.DeleteFileAsync("/uploads/old.jpg"))
            .ReturnsAsync(Result<bool>.Success(true));

        var updatedUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Updated bio",
            ProfilePictureUrl = uploadedUrl
        };

        _mockUserService.Setup(s => s.UpdateProfileAsync(userId, "Updated bio", uploadedUrl))
            .ReturnsAsync(Result<User>.Success(updatedUser));

        // Act
        var result = await _controller.Edit(model) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Index");
        result.ControllerName.Should().Be("Profile");
        _mockFileService.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), fileName, "image/jpeg"), Times.Once);
        _mockFileService.Verify(s => s.DeleteFileAsync("/uploads/old.jpg"), Times.Once);
        _mockUserService.Verify(s => s.UpdateProfileAsync(userId, "Updated bio", uploadedUrl), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_FileUploadFails_ReturnsViewWithError()
    {
        // Arrange
        var userId = 1;
        SetupAuthenticatedUser(userId);

        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var model = new EditProfileViewModel
        {
            Bio = "Updated bio",
            ProfilePicture = fileMock.Object
        };

        _mockFileService.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "test.jpg", "image/jpeg"))
            .ReturnsAsync(Result<string>.Failure("File upload failed", ErrorCodes.Validation));

        // Act
        var result = await _controller.Edit(model) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().Be(model);
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        _mockUserService.Verify(s => s.UpdateProfileAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Post_UserServiceFails_ReturnsViewWithError()
    {
        // Arrange
        var userId = 1;
        SetupAuthenticatedUser(userId);

        var model = new EditProfileViewModel
        {
            Bio = "Updated bio"
        };

        _mockUserService.Setup(s => s.UpdateProfileAsync(userId, "Updated bio", null))
            .ReturnsAsync(Result<User>.Failure("Update failed", ErrorCodes.Validation));

        // Act
        var result = await _controller.Edit(model) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().Be(model);
        _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    #endregion
}
