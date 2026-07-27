using FluentAssertions;
using NexCommerce.Infrastructure.Security;

namespace NexCommerce.Infrastructure.Tests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatches()
    {
        var hash = _sut.Hash("SenhaForte@123");

        _sut.Verify("SenhaForte@123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        var hash = _sut.Hash("SenhaForte@123");

        _sut.Verify("SenhaErrada@456", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_ShouldProduceDifferentHashes_ForSamePassword()
    {
        var hash1 = _sut.Hash("SenhaForte@123");
        var hash2 = _sut.Hash("SenhaForte@123");

        hash1.Should().NotBe(hash2);
    }
}