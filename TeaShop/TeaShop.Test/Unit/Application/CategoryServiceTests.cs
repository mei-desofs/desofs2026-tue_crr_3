using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaShop.Application.Products;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Products;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Test.Unit.Application;

public class CategoryServiceTests
{
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_categories, NullLogger<CategoryService>.Instance);
    }

    // Helpers 

    private static ClaimsPrincipal AdminCaller() => new(
        new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
             new Claim(ClaimTypes.Role, Roles.Admin)],
            authenticationType: "Test"));

    private static ClaimsPrincipal CustomerCaller() => new(
        new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
             new Claim(ClaimTypes.Role, Roles.Customer)],
            authenticationType: "Test"));

    private static ClaimsPrincipal UnauthenticatedCaller() => new(new ClaimsIdentity());

    // CreateAsync 

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldReturnCreatedCategory()
    {
        var req = new CreateCategoryRequest("Green Tea", "A refreshing green tea.");
        _categories.ExistsByNameAsync("Green Tea", TestContext.Current.CancellationToken).Returns(false);

        var result = await _sut.CreateAsync(req, AdminCaller(), TestContext.Current.CancellationToken);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Green Tea");
        result.Description.Should().Be("A refreshing green tea.");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldPersistCategory()
    {
        var req = new CreateCategoryRequest("Black Tea", null);
        _categories.ExistsByNameAsync("Black Tea", TestContext.Current.CancellationToken).Returns(false);

        await _sut.CreateAsync(req, AdminCaller(), TestContext.Current.CancellationToken);

        await _categories.Received(1).AddAsync(Arg.Any<Category>(), TestContext.Current.CancellationToken);
        await _categories.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ShouldThrowConflictException()
    {
        var req = new CreateCategoryRequest("Green Tea", null);
        _categories.ExistsByNameAsync("Green Tea", TestContext.Current.CancellationToken).Returns(true);

        var act = async () => await _sut.CreateAsync(req, AdminCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConflictException>();
        await _categories.DidNotReceive().AddAsync(Arg.Any<Category>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CreateAsync_UnauthenticatedCaller_ShouldThrowUnauthorizedException()
    {
        var req = new CreateCategoryRequest("Green Tea", null);

        var act = async () => await _sut.CreateAsync(req, UnauthenticatedCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task CreateAsync_NonAdminCaller_ShouldThrowForbiddenException()
    {
        var req = new CreateCategoryRequest("Green Tea", null);

        var act = async () => await _sut.CreateAsync(req, CustomerCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // UpdateAsync 

    [Fact]
    public async Task UpdateAsync_ValidRequest_ShouldReturnUpdatedCategory()
    {
        var existing = Category.Create("Old Name", "Old desc.");
        _categories.FindByIdAsync(existing.Id, TestContext.Current.CancellationToken).Returns(existing);
        _categories.ExistsByNameAsync("New Name", TestContext.Current.CancellationToken).Returns(false);
        var req = new UpdateCategoryRequest("New Name", "New desc.");

        var result = await _sut.UpdateAsync(existing.Id, req, AdminCaller(), TestContext.Current.CancellationToken);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New desc.");
        await _categories.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_SameNameCaseInsensitive_ShouldNotCheckForDuplicate()
    {
        var existing = Category.Create("Green Tea");
        _categories.FindByIdAsync(existing.Id, TestContext.Current.CancellationToken).Returns(existing);
        var req = new UpdateCategoryRequest("green tea", null);

        await _sut.UpdateAsync(existing.Id, req, AdminCaller(), TestContext.Current.CancellationToken);

        await _categories.DidNotReceive().ExistsByNameAsync(Arg.Any<string>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_CategoryNotFound_ShouldThrowNotFoundException()
    {
        _categories.FindByIdAsync(Arg.Any<Guid>(), TestContext.Current.CancellationToken).Returns((Category?)null);
        var req = new UpdateCategoryRequest("New Name", null);

        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), req, AdminCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ShouldThrowConflictException()
    {
        var existing = Category.Create("Old Name");
        _categories.FindByIdAsync(existing.Id, TestContext.Current.CancellationToken).Returns(existing);
        _categories.ExistsByNameAsync("Taken Name", TestContext.Current.CancellationToken).Returns(true);
        var req = new UpdateCategoryRequest("Taken Name", null);

        var act = async () => await _sut.UpdateAsync(existing.Id, req, AdminCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConflictException>();
        await _categories.DidNotReceive().SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_UnauthenticatedCaller_ShouldThrowUnauthorizedException()
    {
        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdateCategoryRequest(null, null), UnauthenticatedCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task UpdateAsync_NonAdminCaller_ShouldThrowForbiddenException()
    {
        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdateCategoryRequest(null, null), CustomerCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // DeleteAsync 

    [Fact]
    public async Task DeleteAsync_ExistingCategory_ShouldRemoveAndSave()
    {
        var existing = Category.Create("Oolong Tea");
        _categories.FindByIdAsync(existing.Id, TestContext.Current.CancellationToken).Returns(existing);

        await _sut.DeleteAsync(existing.Id, AdminCaller(), TestContext.Current.CancellationToken);

        await _categories.Received(1).RemoveAsync(existing, TestContext.Current.CancellationToken);
        await _categories.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_CategoryNotFound_ShouldThrowNotFoundException()
    {
        _categories.FindByIdAsync(Arg.Any<Guid>(), TestContext.Current.CancellationToken).Returns((Category?)null);

        var act = async () => await _sut.DeleteAsync(Guid.NewGuid(), AdminCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
        await _categories.DidNotReceive().RemoveAsync(Arg.Any<Category>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_UnauthenticatedCaller_ShouldThrowUnauthorizedException()
    {
        var act = async () => await _sut.DeleteAsync(Guid.NewGuid(), UnauthenticatedCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task DeleteAsync_NonAdminCaller_ShouldThrowForbiddenException()
    {
        var act = async () => await _sut.DeleteAsync(Guid.NewGuid(), CustomerCaller(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
