using TeaShop.Application.Catalog.DTOs;
using TeaShop.Domain.Catalog;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Application.Catalog;

public sealed class CatalogService
{
    private readonly ITeaRepository _teaRepository;

    public CatalogService(ITeaRepository teaRepository)
    {
        _teaRepository = teaRepository;
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
}