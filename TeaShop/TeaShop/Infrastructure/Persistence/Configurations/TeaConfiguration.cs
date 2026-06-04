using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.Catalog;

namespace TeaShop.Infrastructure.Persistence.Configurations;

public sealed class TeaConfiguration : IEntityTypeConfiguration<Tea>
{
    public void Configure(EntityTypeBuilder<Tea> b)
    {
        b.HasKey(t => t.Id);

        b.Property(t => t.Name)
         .HasMaxLength(150)
         .IsRequired();

        b.Property(t => t.Price)
         .IsRequired()
         .HasPrecision(18, 2);

        b.Property(t => t.Stock)
         .IsRequired();

        b.Property(t => t.CategoryId)
         .IsRequired();

     
    }
}