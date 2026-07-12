using NexCommerce.Domain.Enums;
using NexCommerce.Domain.Exceptions;

namespace NexCommerce.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.Customer;
    public bool IsActive { get; private set; } = true;

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(string email, string passwordHash, string fullName, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email é obrigatório.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("Senha é obrigatória.");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Role = role
        };
    }

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }

    public void RevokeAllTokens(string reason = "Revogado pelo sistema.")
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(reason);
        }
        Touch();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new DomainException("Nova senha é obrigatória.");

        PasswordHash = newPasswordHash;
        RevokeAllTokens("Senha alterada.");
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        RevokeAllTokens("Usuário desativado.");
        Touch();
    }
}