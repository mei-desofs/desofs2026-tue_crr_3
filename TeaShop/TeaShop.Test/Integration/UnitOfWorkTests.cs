using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.Products;
using TeaShop.Infrastructure.Persistence;
using TeaShop.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TeaShop.IntegrationTests;

public sealed class UnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TeaShopDbContext _context;

    public UnitOfWorkTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TeaShopDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TeaShopDbContext(options);

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CommitAsync_ShouldPersistChanges()
    {
        var unitOfWork = new UnitOfWork(_context);

        var category = Category.Create("Test Category");

        await _context.Categories.AddAsync(category);

        await _context.SaveChangesAsync();

        var tea = Tea.Create(
            "Green Tea",
            10m,
            5,
            category.Id);

        await unitOfWork.BeginTransactionAsync(CancellationToken.None);

        await _context.Teas.AddAsync(tea);

        await _context.SaveChangesAsync();

        await unitOfWork.CommitAsync(CancellationToken.None);

        var exists = await _context.Teas
            .AnyAsync(x => x.Id == tea.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task RollbackAsync_ShouldUndoChanges()
    {
        var unitOfWork = new UnitOfWork(_context);

        var category = Category.Create("Test Category");

        await _context.Categories.AddAsync(category);

        await _context.SaveChangesAsync();

        var tea = Tea.Create(
            "Black Tea",
            15m,
            10,
            category.Id);

        await unitOfWork.BeginTransactionAsync(CancellationToken.None);

        await _context.Teas.AddAsync(tea);

        await _context.SaveChangesAsync();

        await unitOfWork.RollbackAsync(CancellationToken.None);

        var exists = await _context.Teas
            .AnyAsync(x => x.Id == tea.Id);

        exists.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}