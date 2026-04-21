using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClothesAccessoriesApp.Models;

/// <summary>
/// Модель материала, из которого изготовлен товар.
/// </summary>
public class Material
{
    /// <summary>
    /// Уникальный идентификатор материала.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название материала (обязательное, уникальное поле).
    /// </summary>
    [Required(ErrorMessage = "Название материала обязательно")]
    [MaxLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция товаров, изготовленных из данного материала.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>
    /// Переопределение метода ToString для удобного отображения.
    /// </summary>
    public override string ToString() => Name;
}
