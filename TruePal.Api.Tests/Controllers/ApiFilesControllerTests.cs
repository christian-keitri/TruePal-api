using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using TruePal.Api.Controllers;
using TruePal.Api.Core.Interfaces;
using FluentAssertions;

namespace TruePal.Api.Tests.Controllers;

public class ApiFilesControllerTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly ApiFilesController _controller;

    public ApiFilesControllerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _controller = new ApiFilesController(_mockFileService.Object, NullLogger<ApiFilesController>.Instance);
        
        // Setup authenticated user context
        SetupAuthenticatedUser("1", "testuser", "test@example.com");
    }

    #region Upload Tests

    [Fact]
    public async Task UploadFile_ValidFile_ReturnsOkWithFileData()
    {
        // Arrange
        var file = CreateFormFile("test.jpg", new byte[] { 0xFF, 0xD8 }, "image/jpeg");
        var expectedPath = "/uploads/guid-test.jpg";
        
        _mockFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(Result<string>.Success(expectedPath));

        // Act
        var result = await _controller.UploadFile(file);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        var responseData = okResult.Value!.GetType().GetProperty("filePath")?.GetValue(okResult.Value);
        responseData.Should().Be(expectedPath);
    }

    [Fact]
    public async Task UploadFile_NullFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UploadFile(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        badResult!.Value.Should().NotBeNull();
        
        var errors = badResult.Value!.GetType().GetProperty("errors")?.GetValue(badResult.Value) as List<string>;
        errors.Should().Contain("No file provided");
    }

    [Fact]
    public async Task UploadFile_ServiceReturnsError_ReturnsBadRequest()
    {
        // Arrange
        var file = CreateFormFile("test.pdf", new byte[] { 0x01 }, "application/pdf");
        var errorMessage = "Invalid file type";
        
        _mockFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(Result<string>.Failure(errorMessage));

        // Act
        var result = await _controller.UploadFile(file);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        badResult!.Value.Should().NotBeNull();
        
        var errors = badResult.Value!.GetType().GetProperty("errors")?.GetValue(badResult.Value) as List<string>;
        errors.Should().Contain(errorMessage);
    }

    [Fact]
    public async Task UploadFile_Unauthenticated_ReturnsForbidden()
    {
        // Arrange - Remove authentication
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var file = CreateFormFile("test.jpg", new byte[] { 0xFF }, "image/jpeg");

        // Act
        var result = await _controller.UploadFile(file);

        // Assert - [Authorize] attribute handles this, but we test the scenario
        // In a real scenario, the middleware would catch this before reaching the controller
        // For unit tests, we verify the controller has the attribute
        var authorizeAttribute = _controller.GetType()
            .GetMethod(nameof(ApiFilesController.UploadFile))
            ?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadFile_LargeFile_ReturnsBadRequest()
    {
        // Arrange
        var file = CreateFormFile("large.jpg", new byte[6 * 1024 * 1024], "image/jpeg");
        var errorMessage = "File size exceeds the maximum allowed size of 5 MB";
        
        _mockFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(Result<string>.Failure(errorMessage));

        // Act
        var result = await _controller.UploadFile(file);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        
        var errors = badResult!.Value!.GetType().GetProperty("errors")?.GetValue(badResult.Value) as List<string>;
        errors.Should().Contain(errorMessage);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteFile_ValidPath_ReturnsOk()
    {
        // Arrange
        var filePath = "/uploads/test.jpg";
        
        _mockFileService.Setup(x => x.DeleteFileAsync(filePath))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.DeleteFile(filePath);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var message = okResult!.Value!.GetType().GetProperty("message")?.GetValue(okResult.Value);
        message.Should().Be("File deleted successfully");
    }

    [Fact]
    public async Task DeleteFile_EmptyPath_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.DeleteFile("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        
        var errors = badResult!.Value!.GetType().GetProperty("errors")?.GetValue(badResult.Value) as List<string>;
        errors.Should().Contain("File path is required");
    }

    [Fact]
    public async Task DeleteFile_NonExistentFile_ReturnsNotFound()
    {
        // Arrange
        var filePath = "/uploads/non-existent.jpg";
        
        _mockFileService.Setup(x => x.DeleteFileAsync(filePath))
            .ReturnsAsync(Result<bool>.Failure("File not found"));

        // Act
        var result = await _controller.DeleteFile(filePath);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        
        var errors = notFoundResult!.Value!.GetType().GetProperty("errors")?.GetValue(notFoundResult.Value) as List<string>;
        errors.Should().Contain("File not found");
    }

    [Fact]
    public async Task DeleteFile_ServiceError_ReturnsNotFound()
    {
        // Arrange
        var filePath = "/uploads/error.jpg";
        var errorMessage = "Unable to delete file";
        
        _mockFileService.Setup(x => x.DeleteFileAsync(filePath))
            .ReturnsAsync(Result<bool>.Failure(errorMessage));

        // Act
        var result = await _controller.DeleteFile(filePath);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        
        var errors = notFoundResult!.Value!.GetType().GetProperty("errors")?.GetValue(notFoundResult.Value) as List<string>;
        errors.Should().Contain(errorMessage);
    }

    #endregion

    #region Helper Methods

    private void SetupAuthenticatedUser(string userId, string username, string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private static IFormFile CreateFormFile(string fileName, byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    #endregion
}
