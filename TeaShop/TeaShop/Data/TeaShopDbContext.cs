using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;

namespace TeaShop.Data;

public sealed class TeaShopDbContext : DbContext
{
    public TeaShopDbContext(DbContextOptions<TeaShopDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeaShopDbContext).Assembly);
    }
}