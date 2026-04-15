using TruePal.Api.Core.Interfaces;

namespace TruePal.Api.Application.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private readonly string _uploadPath;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedContentTypes = 
    { 
        "image/jpeg", 
        "image/png", 
        "image/gif", 
        "image/webp" 
    };

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadPath = Path.Combine(_environment.WebRootPath, "uploads");

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created uploads directory at {Path}", _uploadPath);
        }
    }

    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Validation
            var validationErrors = new List<string>();

            // Validate file name
            if (string.IsNullOrWhiteSpace(fileName))
            {
                validationErrors.Add("File name is required");
            }

            // Validate extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                validationErrors.Add($"File type {extension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }

            // Validate content type
            if (!_allowedContentTypes.Contains(contentType.ToLowerInvariant()))
            {
                validationErrors.Add($"Content type {contentType} is not allowed");
            }

            // Validate file size
            if (fileStream.Length > _maxFileSize)
            {
                validationErrors.Add($"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB");
            }

            if (fileStream.Length == 0)
            {
                validationErrors.Add("File is empty");
            }

            if (validationErrors.Any())
            {
                return Result<string>.Failure(validationErrors);
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Save file
            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);

            // Return relative path for storing in database
            var relativePath = $"/uploads/{uniqueFileName}";
            return Result<string>.Success(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return Result<string>.Failure("An error occurred while uploading the file");
        }
    }

    public async Task<Result<bool>> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Result<bool>.Failure("File path is required");
            }

            // Remove leading slash if present
            var relativePath = filePath.TrimStart('/');
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                return Result<bool>.Failure("File not found", ErrorCodes.NotFound);
            }

            await Task.Run(() => File.Delete(fullPath));
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Result<bool>.Failure("An error occurred while deleting the file");
        }
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var relativePath = filePath.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
        return File.Exists(fullPath);
    }

    public string GetFileUrl(string filePath)
    {
        // Returns the relative URL that can be used in HTML
        return filePath.StartsWith("/") ? filePath : $"/{filePath}";
    }
}
