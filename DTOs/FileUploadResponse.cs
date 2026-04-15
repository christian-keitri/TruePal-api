namespace TruePal.Api.DTOs;

public class FileUploadResponse
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;

    public static FileUploadResponse Create(string filePath, string fileName, string fileUrl)
    {
        return new FileUploadResponse
        {
            FilePath = filePath,
            FileName = fileName,
            FileUrl = fileUrl
        };
    }
}
