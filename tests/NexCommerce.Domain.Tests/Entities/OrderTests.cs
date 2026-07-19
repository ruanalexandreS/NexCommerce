using FluentAssertions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;
using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Tests.Entities;

public class OrderTests
{
    private static Product CreateProduct(decimal price = 100m) =>
        Product.Create("Mouse", "MS-001", price, 50);

    private static Order CreateSut() => Order.Create(Guid.CreateVersion7());

    [Fact]
    public void AddItem_ShouldRecalculateTotal()
    {
        var order = CreateSut();

        order.AddItem(CreateProduct(price: 100m), quantity: 3);

        order.Total.Should().Be(300m);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_ShouldThrow_WhenProductAlreadyAdded()
    {
        var order = CreateSut();
        var product = CreateProduct();

        order.AddItem(product, 1);
        var act = () => order.AddItem(product, 2);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_ShouldThrow_WhenOrderHasNoItems()
    {
        var order = CreateSut();

        var act = () => order.MarkAsPaid("pi_123");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_ShouldThrow_WhenCalledTwice()
    {
        var order = CreateSut();
        order.AddItem(CreateProduct(), 1);
        order.MarkAsPaid("pi_123");

        var act = () => order.MarkAsPaid("pi_456");

        act.Should().Throw<DomainException>();
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentIntentId.Should().Be("pi_123");
    }

    private static Order CreatePaidOrder()
    {
        var order = CreateSut();
        order.AddItem(CreateProduct(), 1);
        order.MarkAsPaid("pi_123");
        return order;
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent()
    {
        var order = CreateSut();
        order.AddItem(CreateProduct(), 1);

        order.Cancel();
        var act = () => order.Cancel();

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenOrderIsShipped()
    {
        var order = CreatePaidOrder();
        order.Ship();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Ship_ShouldThrow_WhenOrderIsNotPaid()
    {
        var order = CreateSut();
        order.AddItem(CreateProduct(), 1);

        var act = () => order.Ship();

        act.Should().Throw<DomainException>();
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Deliver_ShouldThrow_WhenOrderIsNotShipped()
    {
        var order = CreatePaidOrder();

        var act = () => order.Deliver();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deliver_ShouldSucceed_WhenOrderIsShipped()
    {
        var order = CreatePaidOrder();
        order.Ship();

        order.Deliver();

        order.Status.Should().Be(OrderStatus.Delivered);
    }
}