using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeaShop.Domain.IAM;

namespace TeaShop.Data.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s => s.Token)
         .HasConversion(
             token => token.Value,
             raw => SessionToken.FromExisting(raw))
         .HasMaxLength(128)
         .IsRequired();

        b.HasIndex(s => s.Token).IsUnique();

        b.Property(s => s.UserId).IsRequired();
        b.Property(s => s.UserRole).HasMaxLength(20).IsRequired();
        b.Property(s => s.CreatedAt).IsRequired();
        b.Property(s => s.ExpiresAt).IsRequired();
        b.Property(s => s.IsRevoked).IsRequired();
    }
}