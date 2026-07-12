namespace NexCommerce.Domain.Interfaces;

public interface ITokenHasher
{
    string Hash(string token);
}