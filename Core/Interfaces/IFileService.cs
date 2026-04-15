namespace TruePal.Api.Core.Interfaces;

public interface IFileService
{
    Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Result<bool>> DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
    string GetFileUrl(string filePath);
}
