using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;
using TeaShop.Domain.Catalog;

namespace TeaShop.Infrastructure.Data;

public sealed class TeaShopDbContext : DbContext
{
    public TeaShopDbContext(DbContextOptions<TeaShopDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<Tea> Teas { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeaShopDbContext).Assembly);
    }
}