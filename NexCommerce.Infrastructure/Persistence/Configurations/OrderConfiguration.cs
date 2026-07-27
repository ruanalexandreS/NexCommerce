using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Total).HasPrecision(18, 2);
        builder.Property(o => o.Status).HasConversion<byte>();
        builder.Property(o => o.PaymentIntentId).HasMaxLength(100);
        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.HasIndex(o => new { o.UserId, o.CreatedAt });
        builder.HasIndex(o => o.PaymentIntentId).IsUnique().HasFilter("[PaymentIntentId] IS NOT NULL");

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}