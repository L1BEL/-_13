using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Models;
using ClothesAccessoriesApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClothesAccessoriesApp.Tests.Repositories;

public class ProductRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsProduct()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var category = new Category { Name = "Кроссовки" };
        var brand = new Brand { Name = "Velocity" };
        var material = new Material { Name = "Текстиль" };
        context.Categories.Add(category);
        context.Brands.Add(brand);
        context.Materials.Add(material);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var product = new Product
        {
            Name = "Sprint Pro X",
            Description = "Test",
            Price = 9900m,
            StockQuantity = 7,
            CategoryId = category.Id,
            BrandId = brand.Id,
            MaterialId = material.Id
        };

        await repository.AddAsync(product);

        Assert.Equal(1, await context.Products.CountAsync());
    }
}
