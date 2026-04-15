using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TruePal.Api.Application.Services;
using TruePal.Api.Core.Interfaces;
using FluentAssertions;

namespace TruePal.Api.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly FileService _fileService;
    private readonly string _testUploadsPath;

    public FileServiceTests()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        mockEnv.Setup(e => e.WebRootPath).Returns(webRootPath);
        
        _fileService = new FileService(mockEnv.Object, NullLogger<FileService>.Instance);
        _testUploadsPath = Path.Combine(webRootPath, "uploads");
        
        // Ensure uploads directory exists for tests
        Directory.CreateDirectory(_testUploadsPath);
    }

    public void Dispose()
    {
        // Clean up test files
        if (Directory.Exists(_testUploadsPath))
        {
            var testFiles = Directory.GetFiles(_testUploadsPath, "test-*.*");
            foreach (var file in testFiles)
            {
                try { File.Delete(file); } catch { /* Ignore cleanup errors */ }
            }
        }
    }

    [Fact]
    public async Task UploadFileAsync_ValidImage_ReturnsSuccess()
    {
        // Arrange
        var fileName = "test-image.jpg";
        var content = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
        var stream = new MemoryStream(content);

        // Act
        var result = await _fileService.UploadFileAsync(stream, fileName, "image/jpeg");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().StartWith("/uploads/");
        result.Data.Should().EndWith(".jpg");

        // Cleanup
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Data.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task UploadFileAsync_NullFile_ReturnsError()
    {
        // Act - Null stream will cause exception caught by FileService
        var result = await _fileService.UploadFileAsync(null!, "test.jpg", "image/jpeg");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An error occurred while uploading the file");
    }

    [Fact]
    public async Task UploadFileAsync_EmptyFile_ReturnsError()
    {
        // Arrange
        var stream = new MemoryStream(Array.Empty<byte>());

        // Act
        var result = await _fileService.UploadFileAsync(stream, "empty.jpg", "image/jpeg");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("File is empty");
    }

    [Fact]
    public async Task UploadFileAsync_InvalidExtension_ReturnsError()
    {
        // Arrange
        var content = new byte[] { 0x01, 0x02, 0x03 };
        var stream = new MemoryStream(content);

        // Act
        var result = await _fileService.UploadFileAsync(stream, "document.pdf", "application/pdf");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("File type .pdf is not allowed"));
    }

    [Fact]
    public async Task UploadFileAsync_InvalidContentType_ReturnsError()
    {
        // Arrange
        var content = new byte[] { 0x01, 0x02, 0x03 };
        var stream = new MemoryStream(content);

        // Act
        var result = await _fileService.UploadFileAsync(stream, "image.jpg", "application/octet-stream");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Content type application/octet-stream is not allowed"));
    }

    [Fact]
    public async Task UploadFileAsync_ExceedsMaxSize_ReturnsError()
    {
        // Arrange
        var largeContent = new byte[6 * 1024 * 1024]; // 6MB
        var stream = new MemoryStream(largeContent);

        // Act
        var result = await _fileService.UploadFileAsync(stream, "large.jpg", "image/jpeg");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("File size exceeds maximum allowed size"));
    }

    [Fact]
    public async Task UploadFileAsync_WebpImage_ReturnsSuccess()
    {
        // Arrange
        var content = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // RIFF header for WebP
        var stream = new MemoryStream(content);

        // Act
        var result = await _fileService.UploadFileAsync(stream, "test-image.webp", "image/webp");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().StartWith("/uploads/");
        result.Data.Should().EndWith(".webp");

        // Cleanup
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Data.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_ReturnsSuccess()
    {
        // Arrange - Create a test file
        var testFilePath = "/uploads/test-file-to-delete.jpg";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", testFilePath.TrimStart('/'));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, new byte[] { 0x01, 0x02, 0x03 });

        // Act
        var result = await _fileService.DeleteFileAsync(testFilePath);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistentFile_ReturnsError()
    {
        // Act
        var result = await _fileService.DeleteFileAsync("/uploads/non-existent-file.jpg");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("File not found");
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        // Arrange - Create a test file
        var testFilePath = "/uploads/test-exists.jpg";
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", testFilePath.TrimStart('/'));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, new byte[] { 0x01 });

        try
        {
            // Act
            var exists = _fileService.FileExists(testFilePath);

            // Assert
            exists.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    [Fact]
    public void FileExists_NonExistentFile_ReturnsFalse()
    {
        // Act
        var exists = _fileService.FileExists("/uploads/does-not-exist.jpg");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void GetFileUrl_ValidPath_ReturnsCorrectUrl()
    {
        // Act
        var url = _fileService.GetFileUrl("/uploads/test.jpg");

        // Assert
        url.Should().Be("/uploads/test.jpg");
    }

    // Helper method to create IFormFile
    private static IFormFile CreateFormFile(string fileName, byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
