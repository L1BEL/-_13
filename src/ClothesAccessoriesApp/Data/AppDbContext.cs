using ClothesAccessoriesApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothesAccessoriesApp.Data;

public class AppDbContext : DbContext
{
    private readonly string? _connectionString;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Material> Materials => Set<Material>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && !string.IsNullOrWhiteSpace(_connectionString))
        {
            optionsBuilder.UseSqlite($"Data Source={_connectionString}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasIndex(item => item.Name).IsUnique();
        modelBuilder.Entity<Brand>().HasIndex(item => item.Name).IsUnique();
        modelBuilder.Entity<Material>().HasIndex(item => item.Name).IsUnique();
        modelBuilder.Entity<Product>().Property(item => item.Price).HasPrecision(10, 2);
    }
}
