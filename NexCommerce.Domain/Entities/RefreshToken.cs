using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public string TokenHash { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("TokenHash é obrigatório.");
        if (lifetime <= TimeSpan.Zero) throw new DomainException("Lifetime deve ser positivo.");

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        };
    }

    public void Revoke(string reason)
    {
        if (RevokedAt is not null) return;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        Touch();
    }
}