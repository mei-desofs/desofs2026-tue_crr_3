using FluentAssertions;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.Exceptions;
using Xunit;

namespace TeaShop.Test.Unit.Domain.Catalog;

public class TeaTests
{
    private const string ValidName = "Earl Grey";
    private const decimal ValidPrice = 5.99m;
    private const int ValidStock = 50;
    private static readonly Guid ValidCategoryId = Guid.NewGuid();


    [Fact]
    public void Create_ValidInputs_ShouldSucceed()
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        tea.Id.Should().NotBeEmpty();
        tea.Name.Should().Be(ValidName);
        tea.Price.Should().Be(ValidPrice);
        tea.Stock.Should().Be(ValidStock);
        tea.CategoryId.Should().Be(ValidCategoryId);
    }

    [Fact]
    public void Create_NameWithWhitespace_ShouldTrimName()
    {
        var untrimmedName = "  Matcha Green Tea  ";

        var tea = Tea.Create(untrimmedName, ValidPrice, ValidStock, ValidCategoryId);

        tea.Name.Should().Be("Matcha Green Tea");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidName_ShouldThrowDomainException(string? invalidName)
    {
        var act = () => Tea.Create(invalidName!, ValidPrice, ValidStock, ValidCategoryId);

        act.Should().Throw<DomainException>().WithMessage("Tea name is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public void Create_InvalidPrice_ShouldThrowDomainException(decimal invalidPrice)
    {
        var act = () => Tea.Create(ValidName, invalidPrice, ValidStock, ValidCategoryId);

        act.Should().Throw<DomainException>().WithMessage("Price must be greater than zero.");
    }

    [Fact]
    public void Create_NegativeStock_ShouldThrowDomainException()
    {
        var act = () => Tea.Create(ValidName, ValidPrice, -1, ValidCategoryId);

        act.Should().Throw<DomainException>().WithMessage("Stock cannot be negative.");
    }



    [Fact]
    public void Update_ValidInputs_ShouldUpdateAllProperties()
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);
        var newCategoryId = Guid.NewGuid();

        tea.Update("English Breakfast ", 6.50m, 100, newCategoryId);

        tea.Name.Should().Be("English Breakfast");
        tea.Price.Should().Be(6.50m);
        tea.Stock.Should().Be(100);
        tea.CategoryId.Should().Be(newCategoryId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_InvalidName_ShouldThrowDomainException(string? invalidName)
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        var act = () => tea.Update(invalidName!, ValidPrice, ValidStock, ValidCategoryId);

        act.Should().Throw<DomainException>().WithMessage("Tea name is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Update_InvalidPrice_ShouldThrowDomainException(decimal invalidPrice)
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        var act = () => tea.Update(ValidName, invalidPrice, ValidStock, ValidCategoryId);

        act.Should().Throw<DomainException>().WithMessage("Price must be greater than zero.");
    }

    [Fact]
    public void Update_NegativeStock_ShouldThrowDomainException()
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        var act = () => tea.Update(ValidName, ValidPrice, -5, ValidCategoryId);

        act.Should().Throw<DomainException>().WithMessage("Stock cannot be negative.");
    }

    [Fact]
    public void Update_EmptyCategoryId_ShouldThrowDomainException()
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        var act = () => tea.Update(ValidName, ValidPrice, ValidStock, Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("Category is required.");
    }

    [Fact]
    public void UpdateStock_ValidStock_ShouldUpdateStockValue()
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        tea.UpdateStock(25);

        tea.Stock.Should().Be(25);
    }

    [Fact]
    public void UpdateStock_NegativeStock_ShouldThrowDomainException()
    {
        var tea = Tea.Create(ValidName, ValidPrice, ValidStock, ValidCategoryId);

        var act = () => tea.UpdateStock(-1);

        act.Should().Throw<DomainException>().WithMessage("Stock cannot be negative.");
    }

}