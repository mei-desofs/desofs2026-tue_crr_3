using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Products;

namespace TeaShop.Test.Unit.Domain.Products;

public class CategoryTests
{
    //  Create 

    [Fact]
    public void Create_ValidNameAndDescription_ShouldSetAllProperties()
    {
        var category = Category.Create("Green Tea", "A refreshing green tea.");

        category.Id.Should().NotBeEmpty();
        category.Name.Should().Be("Green Tea");
        category.Description.Should().Be("A refreshing green tea.");
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithoutDescription_ShouldHaveNullDescription()
    {
        var category = Category.Create("Black Tea");

        category.Description.Should().BeNull();
    }

    [Fact]
    public void Create_NameWithSurroundingWhitespace_ShouldTrimName()
    {
        var category = Category.Create("  Herbal Tea  ");

        category.Name.Should().Be("Herbal Tea");
    }

    [Theory]
    [InlineData("", "Category name is required.")]
    [InlineData("   ", "Category name is required.")]
    public void Create_BlankName_ShouldThrowDomainException(string name, string expectedMessage)
    {
        var act = () => Category.Create(name);

        act.Should().Throw<DomainException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Create_NameExceedsMaxLength_ShouldThrowDomainException()
    {
        var longName = new string('a', 101);

        var act = () => Category.Create(longName);

        act.Should().Throw<DomainException>()
            .WithMessage(FailureMessages.Category.NameTooLong);
    }

    [Fact]
    public void Create_DescriptionExceedsMaxLength_ShouldThrowDomainException()
    {
        var longDescription = new string('d', 501);

        var act = () => Category.Create("Valid Name", longDescription);

        act.Should().Throw<DomainException>()
            .WithMessage(FailureMessages.Category.DescriptionTooLong);
    }

    [Fact]
    public void Create_NameAtExactMaxLength_ShouldSucceed()
    {
        var name = new string('a', 100);

        var act = () => Category.Create(name);

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_DescriptionAtExactMaxLength_ShouldSucceed()
    {
        var description = new string('d', 500);

        var act = () => Category.Create("Valid Name", description);

        act.Should().NotThrow();
    }

    //  Update 

    [Fact]
    public void Update_ValidName_ShouldChangeName()
    {
        var category = Category.Create("Old Name");

        category.Update("New Name", null);

        category.Name.Should().Be("New Name");
    }

    [Fact]
    public void Update_ValidDescription_ShouldChangeDescription()
    {
        var category = Category.Create("Tea", "Old description.");

        category.Update(null, "New description.");

        category.Description.Should().Be("New description.");
    }

    [Fact]
    public void Update_NullName_ShouldNotChangeName()
    {
        var category = Category.Create("Unchanged Name");

        category.Update(null, "Some description.");

        category.Name.Should().Be("Unchanged Name");
    }

    [Fact]
    public void Update_NullDescription_ShouldNotChangeDescription()
    {
        var category = Category.Create("Tea", "Unchanged description.");

        category.Update("New Name", null);

        category.Description.Should().Be("Unchanged description.");
    }

    [Theory]
    [InlineData("", "Category name is required.")]
    [InlineData("   ", "Category name is required.")]
    public void Update_BlankName_ShouldThrowDomainException(string name, string expectedMessage)
    {
        var category = Category.Create("Valid");

        var act = () => category.Update(name, null);

        act.Should().Throw<DomainException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Update_NameExceedsMaxLength_ShouldThrowDomainException()
    {
        var category = Category.Create("Valid");
        var longName = new string('a', 101);

        var act = () => category.Update(longName, null);

        act.Should().Throw<DomainException>()
            .WithMessage(FailureMessages.Category.NameTooLong);
    }

    [Fact]
    public void Update_DescriptionExceedsMaxLength_ShouldThrowDomainException()
    {
        var category = Category.Create("Valid");
        var longDescription = new string('d', 501);

        var act = () => category.Update(null, longDescription);

        act.Should().Throw<DomainException>()
            .WithMessage(FailureMessages.Category.DescriptionTooLong);
    }
}
