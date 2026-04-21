using System.ComponentModel.DataAnnotations;

namespace ClothesAccessoriesApp.Models;

/// <summary>
/// Модель товара в каталоге.
/// </summary>
public class Product
{
    /// <summary>
    /// Уникальный идентификатор товара.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название товара (обязательное поле).
    /// </summary>
    [Required(ErrorMessage = "Название товара обязательно")]
    [MaxLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание товара.
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Цена товара (должна быть положительной).
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
    public decimal Price { get; set; }

    /// <summary>
    /// Количество товара на складе (неотрицательное число).
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Количество должно быть неотрицательным числом")]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Идентификатор категории.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с категорией.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Идентификатор бренда.
    /// </summary>
    public int BrandId { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с брендом.
    /// </summary>
    public Brand? Brand { get; set; }

    /// <summary>
    /// Идентификатор материала.
    /// </summary>
    public int MaterialId { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с материалом.
    /// </summary>
    public Material? Material { get; set; }

    /// <summary>
    /// Путь к изображению товара (относительный или абсолютный).
    /// </summary>
    [MaxLength(500, ErrorMessage = "Путь к изображению не должен превышать 500 символов")]
    public string? ImagePath { get; set; }

    /// <summary>
    /// Переопределение метода ToString для удобного отображения.
    /// </summary>
    public override string ToString() => $"{Name} ({Price:C})";
}
