using FluentAssertions;
using NexCommerce.Application.Common;

namespace NexCommerce.Application.Tests.Common;

public class ResultTests
{
    private static readonly Error SampleError = new("test.error", "Erro de teste.");

    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        var result = Result.Failure(SampleError);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }

    [Fact]
    public void SuccessOfT_ShouldExposeValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ValueOfT_ShouldThrow_WhenResultIsFailure()
    {
        var result = Result<int>.Failure(SampleError);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        Result<int> result = SampleError;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }
}