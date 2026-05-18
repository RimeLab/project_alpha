using Microsoft.EntityFrameworkCore;

namespace AlphaApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Coffee> Coffees => Set<Coffee>();
}
