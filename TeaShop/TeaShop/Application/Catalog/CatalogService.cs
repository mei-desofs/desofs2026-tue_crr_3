using Microsoft.Extensions.Options;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security.Interfaces;

namespace TeaShop.Application.Catalog;

public sealed class CatalogService
{
    private readonly ITeaRepository _teaRepository;
    private readonly IFileUploadService _fileUploadService;

    public CatalogService(
        ITeaRepository teaRepository,
        IFileUploadService fileUploadService)
    {
        _teaRepository = teaRepository;
        _fileUploadService = fileUploadService;
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

        if (tea.Image != null)
        {
            _fileUploadService.DeleteFile(tea.Image.FilePath);
            tea.RemoveImage();
        }

        var securePhysicalPath = await _fileUploadService.SaveFileAsync(file, ct);

        tea.SetImage(file.FileName, securePhysicalPath, file.Length);

        await _teaRepository.SaveChangesAsync(ct);
    }

    public async Task<(byte[] FileBytes, string MimeType, string OriginalName)> GetImageAsync(Guid teaId, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdWithImagesAsync(teaId, ct)
            ?? throw new KeyNotFoundException("Tea not found.");


        if (tea.Image == null)
            throw new NotFoundException("No image associated with this tea.");

        var fileBytes = await _fileUploadService.ReadFileAsync(tea.Image.FilePath, ct);

        var extension = Path.GetExtension(tea.Image.FilePath).ToLowerInvariant();

        string mimeType = extension == ".png" ? "image/png" : "image/jpeg";

        return (fileBytes, mimeType, tea.Image.FileName);
    }

    public async Task DeleteImageAsync(Guid teaId, CancellationToken ct)
    {
        var tea = await _teaRepository.GetByIdWithImagesAsync(teaId, ct)
            ?? throw new NotFoundException("Tea not found.");


        if (tea.Image == null)
            throw new NotFoundException("No image associated with this tea.");

        _fileUploadService.DeleteFile(tea.Image.FilePath);

        tea.RemoveImage();

        await _teaRepository.SaveChangesAsync(ct);
    }

}