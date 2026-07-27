using Microsoft.EntityFrameworkCore;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence;

public sealed class NexCommerceDbContext(DbContextOptions<NexCommerceDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexCommerceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}