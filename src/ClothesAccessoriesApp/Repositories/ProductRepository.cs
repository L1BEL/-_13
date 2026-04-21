using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothesAccessoriesApp.Repositories;

/// <summary>
/// Репозиторий для работы с товарами в базе данных.
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    /// <summary>
    /// Инициализирует новый экземпляр репозитория товаров.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public ProductRepository(AppDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Получает полный каталог товаров с включёнными связанными данными (категория, бренд, материал).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список всех товаров с загруженными связанными данными.</returns>
    public async Task<IReadOnlyList<Product>> GetCatalogAsync(CancellationToken cancellationToken = default) =>
        await Context.Products
            .Include(item => item.Category)
            .Include(item => item.Brand)
            .Include(item => item.Material)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Получает товар по идентификатору с включёнными связанными данными.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Товар или null, если товар не найден.</returns>
    public async Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await Context.Products
            .Include(item => item.Category)
            .Include(item => item.Brand)
            .Include(item => item.Material)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
}
