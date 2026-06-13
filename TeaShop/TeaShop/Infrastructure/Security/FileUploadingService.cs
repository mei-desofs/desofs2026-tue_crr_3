using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Security.Interfaces;

namespace TeaShop.Infrastructure.Security;

public sealed class FileUploadService : IFileUploadService
{
    private readonly ImageStorageSettings _settings;
    private readonly string _resolvedStoragePath;

    public FileUploadService(IOptions<ImageStorageSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.StoragePath))
            throw new DomainException("Image storage path is not configured.");

        if (Path.IsPathRooted(_settings.StoragePath))
            throw new DomainException("Image storage path must be relative.");

        _resolvedStoragePath = Path.GetFullPath(_settings.StoragePath, AppDomain.CurrentDomain.BaseDirectory);

        if (!Directory.Exists(_resolvedStoragePath))
        {
            Directory.CreateDirectory(_resolvedStoragePath);
        }
    }

    // Magic byte signatures
    private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly byte[] JpegSignature = { 255, 216, 255 };

    public async Task<string> SaveFileAsync(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            throw new DomainException("File is empty.");

        if (file.Length > _settings.MaxFileSizeInBytes)
            throw new DomainException("File size exceeds the allowed limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            throw new DomainException("Invalid file extension.");

        using (var stream = file.OpenReadStream())
        {
            var buffer = new byte[8];

            await stream.ReadExactlyAsync(buffer, ct);

            if (!IsImageSignatureValid(buffer, extension))
                throw new DomainException("File content does not match the extension signature.");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var combinedPath = Path.Combine(_resolvedStoragePath, uniqueFileName);
        var securePhysicalPath = Path.GetFullPath(combinedPath);

        if (!securePhysicalPath.StartsWith(_resolvedStoragePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Path traversal attempt detected.");
        }

        using (var stream = new FileStream(securePhysicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, ct);
        }

        return securePhysicalPath;
    }

    public async Task<byte[]> ReadFileAsync(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Physical file not found on server.");

        return await File.ReadAllBytesAsync(filePath, ct);
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static bool IsImageSignatureValid(byte[] buffer, string extension)
    {
        if (extension == ".png")
            return buffer.Take(PngSignature.Length).SequenceEqual(PngSignature);

        if (extension == ".jpg" || extension == ".jpeg")
            return buffer.Take(JpegSignature.Length).SequenceEqual(JpegSignature);

        return false;
    }
}