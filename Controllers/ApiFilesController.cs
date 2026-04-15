using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.DTOs;

namespace TruePal.Api.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class ApiFilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<ApiFilesController> _logger;

    public ApiFilesController(IFileService fileService, ILogger<ApiFilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file (requires authentication)
    /// </summary>
    /// <param name="file">The image file to upload (JPG, PNG, GIF, WEBP, max 5MB)</param>
    /// <returns>File upload result with path and URL</returns>
    /// <response code="200">File uploaded successfully</response>
    /// <response code="400">Invalid file or validation error</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Server error during upload</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(FileUploadResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return MapError(null, ErrorCodes.Validation, new List<string> { "No file provided" });
            }

            using (var stream = file.OpenReadStream())
            {
                var result = await _fileService.UploadFileAsync(stream, file.FileName, file.ContentType);

                if (!result.IsSuccess)
                {
                    return MapError(result.Error, result.ErrorCode, result.Errors);
                }

                return Ok(FileUploadResponse.Create(
                    result.Data!,
                    file.FileName,
                    _fileService.GetFileUrl(result.Data!)
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    /// <summary>
    /// Delete a file (requires authentication)
    /// </summary>
    /// <param name="filePath">The relative path of the file to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">File deleted successfully</response>
    /// <response code="400">Invalid file path</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Server error during deletion</response>
    [HttpDelete]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return MapError(null, ErrorCodes.Validation, new List<string> { "File path is required" });
            }

            var result = await _fileService.DeleteFileAsync(filePath);

            if (!result.IsSuccess)
            {
                return MapError(result.Error, result.ErrorCode);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { error = "An error occurred while deleting the file" });
        }
    }

    private IActionResult MapError(string? error, string? errorCode, List<string>? errors = null)
    {
        if (errors != null && errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        return errorCode switch
        {
            ErrorCodes.NotFound => NotFound(new { error }),
            ErrorCodes.Forbidden => StatusCode(403, new { error }),
            ErrorCodes.Validation => BadRequest(new { error }),
            _ => BadRequest(new { error })
        };
    }
}
