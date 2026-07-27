using NexCommerce.Domain.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace NexCommerce.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCryptNet.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) =>
        BCryptNet.Verify(password, passwordHash);
}