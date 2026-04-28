using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.Users;

namespace TeaShop.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);

        b.Property(u => u.Email)
         .HasConversion(
             email => email.Value,
             raw => Email.Create(raw))
         .HasMaxLength(254)
         .IsRequired();

        b.HasIndex(u => u.Email).IsUnique();

        b.Property(u => u.PasswordHash)
         .HasMaxLength(512)
         .IsRequired();

        b.Property(u => u.Role)
         .HasMaxLength(20)
         .IsRequired();

        b.Property(u => u.CreatedAt).IsRequired();

       
        b.OwnsOne(u => u.ShippingAddress, a =>
        {
            a.Property(x => x.Street).HasMaxLength(200).HasColumnName("Address_Street");
            a.Property(x => x.City).HasMaxLength(100).HasColumnName("Address_City");
            a.Property(x => x.PostalCode).HasMaxLength(20).HasColumnName("Address_PostalCode");
            a.Property(x => x.Country).HasMaxLength(100).HasColumnName("Address_Country");
        });
    }
}