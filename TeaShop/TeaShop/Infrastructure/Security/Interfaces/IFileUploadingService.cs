namespace TeaShop.Infrastructure.Security.Interfaces;

public interface IFileUploadService
{
    Task<string> SaveFileAsync(IFormFile file, CancellationToken ct);
    Task<byte[]> ReadFileAsync(string filePath, CancellationToken ct);
    void DeleteFile(string filePath);
}