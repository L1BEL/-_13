using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClothesAccessoriesApp.Models;

/// <summary>
/// Модель бренда (производителя) товаров.
/// </summary>
public class Brand
{
    /// <summary>
    /// Уникальный идентификатор бренда.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название бренда (обязательное, уникальное поле).
    /// </summary>
    [Required(ErrorMessage = "Название бренда обязательно")]
    [MaxLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция товаров данного бренда.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Переопределение метода ToString для удобного отображения.
    /// </summary>
    public override string ToString() => Name;
}
