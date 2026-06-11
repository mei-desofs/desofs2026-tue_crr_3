using Microsoft.Extensions.Options;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Application.Catalog;

public sealed class CatalogService
{
    private readonly ITeaRepository _teaRepository;
    private readonly ImageStorageSettings _settings;
    private readonly string _resolvedStoragePath;

    public CatalogService(
        ITeaRepository teaRepository,
        IOptions<ImageStorageSettings> settings)
    {
        _teaRepository = teaRepository;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.StoragePath))
            throw new DomainValidationException("Image storage path is not configured.");

        if (Path.IsPathRooted(_settings.StoragePath))
            throw new DomainValidationException("Image storage path must be relative.");

        _resolvedStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.StoragePath);

        if (!Directory.Exists(_resolvedStoragePath))
        {
            Directory.CreateDirectory(_resolvedStoragePath);
        }
    }

    public async Task<List<TeaDto>> GetAllAsync(CancellationToken ct)
    {
        var teas = await _teaRepository.GetAllAsync(ct) ?? [];

        return teas.Select(t => new TeaDto(
            t.Id,
            t.Name,
            t.Price,
            t.Stock
        )).ToList();
    }

    public async Task<TeaDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdAsync(id, ct);

        if (tea is null)
            throw new KeyNotFoundException("Tea not found");

        return new TeaDto(
            tea.Id,
            tea.Name,
            tea.Price,
            tea.Stock
        );
    }

    public async Task<List<TeaDto>> GetAllAsync(Guid? categoryId, CancellationToken ct)
    {
        var teas = await _teaRepository.GetAllAsync(ct) ?? [];

        if (categoryId.HasValue)
        {
            var categoryIdValue = categoryId.Value;

            teas = teas
                .Where(t => t.CategoryId == categoryIdValue)
                .ToList();
        }

        return teas.Select(t => new TeaDto(
            t.Id,
            t.Name,
            t.Price,
            t.Stock
        )).ToList();
    }

    public async Task<TeaDto> CreateAsync(CreateTeaRequestDto request, CancellationToken ct)
    {
        var tea = Tea.Create(
            request.Name,
            request.Price,
            request.Stock,
            request.CategoryId
        );

        await _teaRepository.AddAsync(tea, ct);

        return new TeaDto(
            tea.Id,
            tea.Name,
            tea.Price,
            tea.Stock
        );
    }

    public async Task<TeaDto> UpdateAsync(Guid id, UpdateTeaRequestDto request, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdAsync(id, ct);

        if (tea is null)
            throw new KeyNotFoundException("Tea not found.");

        tea.Update(
            request.Name,
            request.Price,
            request.Stock,
            request.CategoryId
        );

        await _teaRepository.SaveChangesAsync(ct);

        return new TeaDto(
            tea.Id,
            tea.Name,
            tea.Price,
            tea.Stock
        );
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdAsync(id, ct);

        if (tea is null)
            throw new KeyNotFoundException("Tea not found.");

        _teaRepository.Remove(tea);
        await _teaRepository.SaveChangesAsync(ct);
    }

    public async Task UploadImageAsync(Guid teaId, IFormFile file, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdWithImagesAsync(teaId, ct)
            ?? throw new KeyNotFoundException("Tea not found.");

        if (file == null || file.Length == 0)
            throw new DomainException("File is empty.");

        if (file.Length > _settings.MaxFileSizeInBytes)
            throw new DomainException("File size exceeds the allowed limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            throw new DomainException("Invalid file extension.");

        if (!_settings.AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new DomainException("Invalid Content-Type.");

        if (tea.Image != null)
        {
            if (File.Exists(tea.Image.FilePath))
            {
                File.Delete(tea.Image.FilePath);
            }
            tea.RemoveImage();
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var securePhysicalPath = Path.Combine(_resolvedStoragePath, uniqueFileName);

        using (var stream = new FileStream(securePhysicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, ct);
        }

        tea.SetImage(file.FileName, securePhysicalPath, file.Length);

        await _teaRepository.SaveChangesAsync(ct);
    }

    public async Task<(byte[] FileBytes, string MimeType, string OriginalName)> GetImageAsync(Guid teaId, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdWithImagesAsync(teaId, ct)
            ?? throw new KeyNotFoundException("Tea not found.");

        if (tea.Image == null)
            throw new KeyNotFoundException("No image associated with this tea.");

        if (!File.Exists(tea.Image.FilePath))
            throw new FileNotFoundException("Physical file not found on server.");

        var fileBytes = await File.ReadAllBytesAsync(tea.Image.FilePath, ct);
        var extension = Path.GetExtension(tea.Image.FilePath).ToLowerInvariant();

        string mimeType;
        if (extension == ".png")
        {
            mimeType = "image/png";
        }
        else
        {
            mimeType = "image/jpeg";
        }

        return (fileBytes, mimeType, tea.Image.FileName);
    }

    public async Task DeleteImageAsync(Guid teaId, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdWithImagesAsync(teaId, ct)
            ?? throw new KeyNotFoundException("Tea not found.");

        if (tea.Image == null)
            throw new KeyNotFoundException("No image associated with this tea.");

        if (File.Exists(tea.Image.FilePath))
        {
            File.Delete(tea.Image.FilePath);
        }

        tea.RemoveImage();

        await _teaRepository.SaveChangesAsync(ct);
    }

}