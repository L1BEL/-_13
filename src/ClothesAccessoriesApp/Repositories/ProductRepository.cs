using System.Collections.Generic;
using System.Threading.Tasks;
using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothesAccessoriesApp.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetCatalogAsync() =>
        await Context.Products
            .Include(item => item.Category)
            .Include(item => item.Brand)
            .Include(item => item.Material)
            .AsNoTracking()
            .ToListAsync();
}
