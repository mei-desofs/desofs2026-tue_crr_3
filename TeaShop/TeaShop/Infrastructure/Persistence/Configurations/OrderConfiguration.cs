using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.Orders;

namespace TeaShop.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.HasKey(o => o.Id);

        b.Property(o => o.UserId).IsRequired();

        b.Property(o => o.Status).IsRequired();

        b.Property(o => o.CreatedAt).IsRequired();

        b.HasMany(o => o.Items)
         .WithOne()
         .HasForeignKey("OrderId")
         .IsRequired()
         .OnDelete(DeleteBehavior.Cascade);
    }
}
