using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClothesAccessoriesApp.Models;

namespace ClothesAccessoriesApp.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с товарами.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Получает полный каталог товаров с включёнными связанными данными.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список всех товаров с загруженными связанными данными.</returns>
    Task<IReadOnlyList<Product>> GetCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает товар по идентификатору с включёнными связанными данными.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Товар или null, если товар не найден.</returns>
    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
