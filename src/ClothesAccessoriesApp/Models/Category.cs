using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClothesAccessoriesApp.Models;

/// <summary>
/// Модель категории товаров.
/// </summary>
public class Category
{
    /// <summary>
    /// Уникальный идентификатор категории.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название категории (обязательное, уникальное поле).
    /// </summary>
    [Required(ErrorMessage = "Название категории обязательно")]
    [MaxLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция товаров в данной категории.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Переопределение метода ToString для удобного отображения.
    /// </summary>
    public override string ToString() => Name;
}
