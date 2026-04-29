using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;

namespace TeaShop.Tests.Unit.Domain;

public class SessionTests
{
    private readonly Guid _userId = Guid.NewGuid();


    [Fact]
    public void Create_ValidInputs_ShouldSucceed()
    {
        var session = Session.Create(_userId, Roles.Customer);

        session.Id.Should().NotBeEmpty();
        session.UserId.Should().Be(_userId);
        session.UserRole.Should().Be(Roles.Customer);
        session.TokenHash.Should().NotBeNullOrWhiteSpace();
        session.IsRevoked.Should().BeFalse();
        session.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Create_InvalidRole_ShouldThrowDomainException()
    {
        var act = () => Session.Create(_userId, "SUPERUSER");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithCustomLifetime_ShouldSetCorrectExpiry()
    {
        var lifetime = TimeSpan.FromHours(1);
        var session = Session.Create(_userId, Roles.Customer, lifetime);

        session.ExpiresAt.Should()
            .BeCloseTo(DateTime.UtcNow.Add(lifetime), TimeSpan.FromSeconds(2));
    }


    [Fact]
    public void IsValid_FreshSession_ShouldReturnTrue()
    {
        var session = Session.Create(_userId, Roles.Customer);
        session.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_RevokedSession_ShouldReturnFalse()
    {
        var session = Session.Create(_userId, Roles.Customer);
        session.Revoke();
        session.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ExpiredSession_ShouldReturnFalse()
    {
        var session = Session.Create(_userId, Roles.Customer, TimeSpan.FromSeconds(-1));
        session.IsValid().Should().BeFalse();
    }


    [Fact]
    public void Revoke_ShouldSetIsRevokedToTrue()
    {
        var session = Session.Create(_userId, Roles.Customer);
        session.Revoke();
        session.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_AlreadyRevoked_ShouldThrowDomainException()
    {
        var session = Session.Create(_userId, Roles.Customer);
        session.Revoke();

        var act = () => session.Revoke();
        act.Should().Throw<DomainException>().WithMessage("Session is already revoked.");
    }

    [Fact]
    public void Create_TwoSessions_ShouldHaveDifferentTokens()
    {
        var a = Session.Create(_userId, Roles.Customer);
        var b = Session.Create(_userId, Roles.Customer);
        a.TokenHash.Should().NotBe(b.TokenHash);
    }
}