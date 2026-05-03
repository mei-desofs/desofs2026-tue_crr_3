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
        var teas = await _teaRepository.GetAllAsync(ct);

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
        var teas = await _teaRepository.GetAllAsync(ct);

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
    }