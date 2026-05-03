using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.Products;

namespace TeaShop.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(c => c.Id);

        b.Property(c => c.Name)
         .HasMaxLength(100)
         .IsRequired();

        b.HasIndex(c => c.Name).IsUnique();

        b.Property(c => c.Description)
         .HasMaxLength(500);

        b.Property(c => c.CreatedAt).IsRequired();
    }
}
