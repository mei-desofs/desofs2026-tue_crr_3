using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeaShop.Domains;

namespace TeaShop.Data
{
    [DbContext(typeof(AppDbContext))]
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(o => o.MigrationsAssembly("TeaShop"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Active)
                    .HasDefaultValue(true);

                entity.HasMany<Order>()
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Active)
                    .HasDefaultValue(true);

                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Active)
                    .HasDefaultValue(true);

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.HasOne(p => p.Stock)
                    .WithOne(s => s.Product)
                    .HasForeignKey<Stock>(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount)
                    .HasPrecision(18, 2);

                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.Payment)
                    .WithOne(p => p.Order)
                    .HasForeignKey<Payment>(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.UnitPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                    .HasPrecision(18, 2);
            });

            // Seed
            modelBuilder.Entity<User>().HasData(new
            {
                Id = -1,
                ExternalId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                FirstName = "App",
                LastName = "Admin",
                Email = "admin@teashop.com",
                PhoneNumber = (string?)null,
                Address = (string?)null,
                CreatedBy = "seed",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedBy = (string?)null,
                UpdatedAt = (DateTime?)null,
                Active = true
            });
        }
    }
}
