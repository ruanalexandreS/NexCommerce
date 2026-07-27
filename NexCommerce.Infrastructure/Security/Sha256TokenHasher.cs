using System.Security.Cryptography;
using System.Text;
using NexCommerce.Domain.Interfaces;

namespace NexCommerce.Infrastructure.Security;

public sealed class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}