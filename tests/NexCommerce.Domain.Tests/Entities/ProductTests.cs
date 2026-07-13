using FluentAssertions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Tests.Entities;

public class ProductTests
{
    private static Product CreateSut(int stock = 10) => Product.Create("Notebook", "nb-001", 4500m, stock);

    [Fact]
    public void Create_ShouldNormalizeSkuToUpperCase()
    {
        var product = CreateSut();
        product.Sku.Should().Be("NB-001");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrow_WhenPriceIsNotPositive(decimal price)
    {
        var act = () => Product.Create("Notebook", "NB-001", price, 10);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void DecreaseStock_ShouldReduceStock_WhenQuantityIsAvailable()
    {
        var product = CreateSut(stock: 10);

        product.DecreaseStock(3);

        product.Stock.Should().Be(7);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void DecreaseStock_ShouldThrowInsufficientStock_WhenQuantityExceedsStock()
    {
        var product = CreateSut(stock: 2);

        var act = () => product.DecreaseStock(5);

        act.Should().Throw<InsufficientStockException>()
           .WithMessage("*NB-001*");
    }
}