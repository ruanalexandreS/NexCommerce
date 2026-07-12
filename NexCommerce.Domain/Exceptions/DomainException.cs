namespace NexCommerce.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public sealed class InsufficientStockException(string sku, int requested, int available) : DomainException($"Estoque insuficiente para Sku '{sku}'. Solicitado: {requested}, disponível: {available}.")
{
    public string Sku { get; } = sku;
}