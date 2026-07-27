using FluentAssertions;
using NexCommerce.Infrastructure.Security;

namespace NexCommerce.Infrastructure.Tests.Security;

public class Sha256TokenHasherTests
{
    private readonly Sha256TokenHasher _sut = new();

    [Fact]
    public void Hash_ShouldBeDeterministic_ForSameInput()
    {
        var hash1 = _sut.Hash("refresh-token-abc");
        var hash2 = _sut.Hash("refresh-token-abc");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_ShouldProduce64LowercaseHexChars()
    {
        var hash = _sut.Hash("refresh-token-abc");

        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Hash_ShouldProduceDifferentHashes_ForDifferentInputs()
    {
        var hash1 = _sut.Hash("token-a");
        var hash2 = _sut.Hash("token-b");

        hash1.Should().NotBe(hash2);
    }
}