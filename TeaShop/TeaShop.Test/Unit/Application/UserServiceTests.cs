using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using TeaShop.Application.Auth;
using TeaShop.Application.UserManagement;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Test.Unit.Application;

public class UserServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly PasswordHashingService _hasher = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_users, _hasher, NullLogger<UserService>.Instance);
    }

    //  Helpers 

    private static User UserWithAddress()
    {
        var user = User.CreateCustomer("user@example.com", "$2a$10$validhashvalue");
        user.UpdateShippingAddress(Address.Create("Rua Dom Dinis", "Porto", "4510-241", "Portugal"));
        return user;
    }

    private static User UserWithoutAddress() =>
        User.CreateCustomer("user@example.com", "$2a$10$validhashvalue");

    private static readonly UpdateAddressRequest ValidAddressRequest =
        new("Avenida dos Aliados, 12", "Porto", "4000-064", "Portugal");

    //  GetAddressAsync 

    [Fact]
    public async Task GetAddressAsync_UserWithAddress_ShouldReturnAddressResponse()
    {
        var user = UserWithAddress();
        _users.FindByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await _sut.GetAddressAsync(user.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Street.Should().Be("Rua Dom Dinis");
        result.City.Should().Be("Porto");
        result.PostalCode.Should().Be("4510-241");
        result.Country.Should().Be("Portugal");
    }

    [Fact]
    public async Task GetAddressAsync_UserWithNoAddress_ShouldReturnNull()
    {
        var user = UserWithoutAddress();
        _users.FindByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await _sut.GetAddressAsync(user.Id, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressAsync_UserNotFound_ShouldThrowNotFoundException()
    {
        _users.FindByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((User?)null);

        var act = async () => await _sut.GetAddressAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(FailureMessages.User.NotFound);
    }

    //  UpdateAddressAsync 

    [Fact]
    public async Task UpdateAddressAsync_ValidRequest_ShouldUpdateAndSave()
    {
        var user = UserWithoutAddress();
        _users.FindByIdAsync(user.Id, CancellationToken.None).Returns(user);

        await _sut.UpdateAddressAsync(user.Id, ValidAddressRequest, CancellationToken.None);

        user.ShippingAddress.Should().NotBeNull();
        user.ShippingAddress!.Street.Should().Be("Avenida dos Aliados, 12");
        await _users.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UpdateAddressAsync_ReplacesExistingAddress()
    {
        var user = UserWithAddress();
        _users.FindByIdAsync(user.Id, CancellationToken.None).Returns(user);
        var newReq = new UpdateAddressRequest("Rua Nova", "Lisbon", "1000-001", "Portugal");

        await _sut.UpdateAddressAsync(user.Id, newReq, CancellationToken.None);

        user.ShippingAddress!.Street.Should().Be("Rua Nova");
        user.ShippingAddress.City.Should().Be("Lisbon");
    }

    [Fact]
    public async Task UpdateAddressAsync_UserNotFound_ShouldThrowNotFoundException()
    {
        _users.FindByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((User?)null);

        var act = async () => await _sut.UpdateAddressAsync(Guid.NewGuid(), ValidAddressRequest, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(FailureMessages.User.NotFound);
        await _users.DidNotReceive().SaveChangesAsync(CancellationToken.None);
    }

    //  RemoveAddressAsync 

    [Fact]
    public async Task RemoveAddressAsync_UserWithAddress_ShouldRemoveAndSave()
    {
        var user = UserWithAddress();
        _users.FindByIdAsync(user.Id, CancellationToken.None).Returns(user);

        await _sut.RemoveAddressAsync(user.Id, CancellationToken.None);

        user.ShippingAddress.Should().BeNull();
        await _users.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RemoveAddressAsync_UserWithNoAddress_ShouldThrowNotFoundException()
    {
        var user = UserWithoutAddress();
        _users.FindByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var act = async () => await _sut.RemoveAddressAsync(user.Id, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(FailureMessages.User.AddressNotFound);
        await _users.DidNotReceive().SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RemoveAddressAsync_UserNotFound_ShouldThrowNotFoundException()
    {
        _users.FindByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((User?)null);

        var act = async () => await _sut.RemoveAddressAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(FailureMessages.User.NotFound);
    }
}
