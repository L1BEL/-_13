using System.Collections.Generic;
using System.Threading.Tasks;
using ClothesAccessoriesApp.Models;

namespace ClothesAccessoriesApp.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetCatalogAsync();
}
