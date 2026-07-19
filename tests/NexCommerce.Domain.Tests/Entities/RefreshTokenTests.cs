using FluentAssertions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Tests.Entities;

public class RefreshTokenTests
{
    private static RefreshToken CreateSut(TimeSpan? lifetime = null) =>
        RefreshToken.Create(Guid.CreateVersion7(), "token-hash", lifetime ?? TimeSpan.FromDays(7));

    [Fact]
    public void Create_ShouldStartActive()
    {
        var token = CreateSut();

        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
        token.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenTokenIsExpired()
    {
        var token = CreateSut(lifetime: TimeSpan.FromMilliseconds(1));

        Thread.Sleep(10);

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenTokenHashIsBlank(string tokenHash)
    {
        var act = () => RefreshToken.Create(Guid.CreateVersion7(), tokenHash, TimeSpan.FromDays(7));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Revoke_ShouldDeactivateToken()
    {
        var token = CreateSut();

        token.Revoke("Logout");

        token.IsActive.Should().BeFalse();
        token.RevokedAt.Should().NotBeNull();
        token.RevokedReason.Should().Be("Logout");
    }

    [Fact]
    public void Revoke_ShouldPreserveOriginalData_WhenCalledTwice()
    {
        var token = CreateSut();
        token.Revoke("Primeiro motivo");
        var firstRevokedAt = token.RevokedAt;

        token.Revoke("Segundo motivo");

        token.RevokedAt.Should().Be(firstRevokedAt);
        token.RevokedReason.Should().Be("Primeiro motivo");
    }
}