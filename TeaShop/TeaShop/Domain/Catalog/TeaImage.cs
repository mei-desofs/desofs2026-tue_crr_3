namespace TeaShop.Domain.Catalog;

public sealed class TeaImage
{
    public Guid Id { get; private set; }
    public Guid TeaId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string FilePath { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private TeaImage() { }

    public static TeaImage Create(Guid teaId, string fileName, string filePath, long sizeBytes)
    {
        return new TeaImage
        {
            Id = Guid.NewGuid(),
            TeaId = teaId,
            FileName = fileName,
            FilePath = filePath,
            SizeBytes = sizeBytes,
            UploadedAt = DateTime.UtcNow
        };
    }
}