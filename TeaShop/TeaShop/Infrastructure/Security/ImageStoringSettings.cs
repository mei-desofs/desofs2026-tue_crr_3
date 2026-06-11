public sealed class ImageStorageSettings
{
    public string StoragePath { get; set; } = "SecureUploadedImages";
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png" };
    public string[] AllowedMimeTypes { get; set; } = { "image/jpeg", "image/png" };
    public long MaxFileSizeInBytes { get; set; } = 5 * 1024 * 1024;
}
