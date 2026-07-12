namespace NexCommerce.Domain.Enums;

public enum OrderStatus : byte
{
    Pending = 1,
    Paid = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}