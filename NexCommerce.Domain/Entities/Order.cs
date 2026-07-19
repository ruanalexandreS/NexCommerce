using NexCommerce.Domain.Enums;
using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Entities;

public sealed class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Total { get; private set; }
    public string? PaymentIntentId { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(Guid userId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId é obrigatório.");
        return new Order { UserId = userId };
    }

    public void AddItem(Product product, int quantity)
    {
        EnsurePending();

        if (!product.IsActive) throw new DomainException($"Produto '{product.Sku}' está inativo.");

        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is not null) throw new DomainException("Produto já adicionado ao pedido.");

        _items.Add(OrderItem.Create(product, quantity));
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Total = _items.Sum(i => i.Subtotal);
        Touch();
    }

    private void EnsurePending()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Pedido não pode ser alterado no status '{Status}'.");
    }

    public void MarkAsPaid(string paymentIntentId)
    {
        EnsurePending();
        if (_items.Count == 0) throw new DomainException("Pedido sem itens não pode ser pago.");

        Status = OrderStatus.Paid;
        PaymentIntentId = paymentIntentId;
        Touch();
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new DomainException($"Pedido '{Status}' não pode ser cancelado.");

        if (Status == OrderStatus.Cancelled) return;

        Status = OrderStatus.Cancelled;
        Touch();
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid) throw new DomainException("Apenas pedidos pagos podem ser enviados.");
        Status = OrderStatus.Shipped;
        Touch();
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped) throw new DomainException("Apenas pedidos enviados podem ser entregues.");
        Status = OrderStatus.Delivered;
        Touch();
    }
}