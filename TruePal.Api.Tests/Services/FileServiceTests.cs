using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
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
        _fileService = new FileService(NullLogger<FileService>.Instance);
        _testUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
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
        var file = CreateFormFile(fileName, content, "image/jpeg");

        // Act
        var result = await _fileService.UploadFileAsync(file);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/uploads/");
        result.Value.Should().EndWith(".jpg");

        // Cleanup
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Value.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task UploadFileAsync_NullFile_ReturnsError()
    {
        // Act
        var result = await _fileService.UploadFileAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("No file was uploaded");
    }

    [Fact]
    public async Task UploadFileAsync_EmptyFile_ReturnsError()
    {
        // Arrange
        var file = CreateFormFile("empty.jpg", Array.Empty<byte>(), "image/jpeg");

        // Act
        var result = await _fileService.UploadFileAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("File is empty");
    }

    [Fact]
    public async Task UploadFileAsync_InvalidExtension_ReturnsError()
    {
        // Arrange
        var content = new byte[] { 0x01, 0x02, 0x03 };
        var file = CreateFormFile("document.pdf", content, "application/pdf");

        // Act
        var result = await _fileService.UploadFileAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid file extension. Only .jpg, .jpeg, .png, .gif, .webp are allowed");
    }

    [Fact]
    public async Task UploadFileAsync_InvalidContentType_ReturnsError()
    {
        // Arrange
        var content = new byte[] { 0x01, 0x02, 0x03 };
        var file = CreateFormFile("image.jpg", content, "application/octet-stream");

        // Act
        var result = await _fileService.UploadFileAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid file type. Only image files are allowed");
    }

    [Fact]
    public async Task UploadFileAsync_ExceedsMaxSize_ReturnsError()
    {
        // Arrange
        var largeContent = new byte[6 * 1024 * 1024]; // 6MB
        var file = CreateFormFile("large.jpg", largeContent, "image/jpeg");

        // Act
        var result = await _fileService.UploadFileAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("File size exceeds the maximum allowed size of 5 MB");
    }

    [Fact]
    public async Task UploadFileAsync_WebpImage_ReturnsSuccess()
    {
        // Arrange
        var content = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // RIFF header for WebP
        var file = CreateFormFile("test-image.webp", content, "image/webp");

        // Act
        var result = await _fileService.UploadFileAsync(file);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/uploads/");
        result.Value.Should().EndWith(".webp");

        // Cleanup
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Value.TrimStart('/'));
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
        result.Errors.Should().Contain("File not found");
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
