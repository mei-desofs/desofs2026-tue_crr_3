using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.Products;

namespace TeaShop.Infrastructure.Persistence.Configurations;

public sealed class TeaConfiguration : IEntityTypeConfiguration<Tea>
{
    public void Configure(EntityTypeBuilder<Tea> builder)
    {
        builder.ToTable("Teas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(t => t.Stock)
            .IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(t => t.Image, imageBuilder =>
        {
            imageBuilder.Property(ti => ti.FileName)
                .HasColumnName("ImageFileName")
                .HasMaxLength(255);

            imageBuilder.Property(ti => ti.FilePath)
                .HasColumnName("ImageFilePath")
                .HasMaxLength(500);

            imageBuilder.Property(ti => ti.SizeBytes)
                .HasColumnName("ImageSizeBytes");

            imageBuilder.Property(ti => ti.UploadedAt)
                .HasColumnName("ImageUploadedAt");
        });
    }

}