using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.Orders;

namespace TeaShop.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.HasKey(i => i.Id);

        b.Property(i => i.TeaId).IsRequired();

        b.Property(i => i.Quantity).IsRequired();

        b.Property(i => i.UnitPrice)
         .IsRequired()
         .HasPrecision(10, 2);
    }
}
