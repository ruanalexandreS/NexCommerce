using FluentAssertions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;
using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Tests.Entities;

public class UserTests
{
    private static User CreateSut() =>
        User.Create("ruan@example.com", "hash-fake", "Ruan Alexandre");

    private static RefreshToken CreateToken(Guid userId) =>
        RefreshToken.Create(userId, "token-hash", TimeSpan.FromDays(7));

    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        var user = User.Create("  Ruan@Example.COM  ", "hash-fake", "Ruan Alexandre");

        user.Email.Should().Be("ruan@example.com");
    }

    [Fact]
    public void Create_ShouldDefaultToCustomerRole()
    {
        var user = CreateSut();

        user.Role.Should().Be(UserRole.Customer);
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenEmailIsBlank(string email)
    {
        var act = () => User.Create(email, "hash-fake", "Ruan Alexandre");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenPasswordHashIsBlank(string passwordHash)
    {
        var act = () => User.Create("ruan@example.com", passwordHash, "Ruan Alexandre");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddRefreshToken_ShouldAddTokenToCollection()
    {
        var user = CreateSut();

        user.AddRefreshToken(CreateToken(user.Id));

        user.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public void ChangePassword_ShouldRevokeAllActiveTokens()
    {
        var user = CreateSut();
        user.AddRefreshToken(CreateToken(user.Id));
        user.AddRefreshToken(CreateToken(user.Id));

        user.ChangePassword("novo-hash");

        user.RefreshTokens.Should().OnlyContain(t => !t.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePassword_ShouldThrow_WhenHashIsBlank(string newPasswordHash)
    {
        var user = CreateSut();

        var act = () => user.ChangePassword(newPasswordHash);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalseAndRevokeTokens()
    {
        var user = CreateSut();
        var token = CreateToken(user.Id);
        user.AddRefreshToken(token);

        user.Deactivate();

        user.IsActive.Should().BeFalse();
        token.IsActive.Should().BeFalse();
    }
}
