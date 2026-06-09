using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Products;
using TeaShop.Domain.Users;
using TeaShop.Domain.Orders;

namespace TeaShop.Infrastructure.Persistence;

public sealed class TeaShopDbContext : DbContext
{
    public TeaShopDbContext(DbContextOptions<TeaShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tea> Teas => Set<Tea>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeaShopDbContext).Assembly);

        if (!Database.IsNpgsql())
        {
            //  Integration Tests
            modelBuilder.Entity<Tea>()
                .Property(t => t.Version)
                .IsConcurrencyToken();
        }
        else
        {
            // For PostgreSQL  native system 'xmin' column
            var versionProperty = modelBuilder.Entity<Tea>()
                .Property(t => t.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}