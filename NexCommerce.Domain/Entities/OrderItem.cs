using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = null!;  // snapshot
    public decimal UnitPrice { get; private set; }            // snapshot
    public int Quantity { get; private set; }

    public decimal Subtotal => UnitPrice * Quantity;

    private OrderItem() { }

    internal static OrderItem Create(Product product, int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantidade deve sser maior que zero.");

        return new OrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = quantity
        };
    }
}