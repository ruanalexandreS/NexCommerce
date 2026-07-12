using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Entities;

public sealed class Product : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = null!;

    private Product() { }

    public static Product Create(string name, string sku, decimal price, int stock)
    {
        if (price <= 0) throw new DomainException("Quantidade deve ser maior que zero.");
        if (stock < 0) throw new DomainException("Estoque não pode ser negativo.");

        return new Product { Name = name.Trim(), Sku = sku.Trim().ToUpperInvariant(), Price = price, Stock = stock };
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantidade deve ser maior que zero.");
        if (quantity > Stock) throw new InsufficientStockException(Sku, quantity, Stock);

        Stock -= quantity;
        Touch();
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantidade deve ser maior que zero.");
        Stock += quantity;
        Touch();
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new DomainException("Preço deve ser maior que zero.");
        Price = newPrice;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}