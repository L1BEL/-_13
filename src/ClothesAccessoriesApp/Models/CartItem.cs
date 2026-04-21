using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClothesAccessoriesApp.Models;

/// <summary>
/// Модель элемента корзины покупок.
/// </summary>
public class CartItem : INotifyPropertyChanged
{
    private int _quantity;

    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Название товара.
    /// </summary>
    [Required(ErrorMessage = "Название товара обязательно")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Цена за единицу товара.
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Количество единиц товара в корзине.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть не меньше 1")]
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value)
            {
                return;
            }

            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPrice));
        }
    }

    /// <summary>
    /// Общая стоимость позиции (цена × количество).
    /// </summary>
    public decimal TotalPrice => UnitPrice * Quantity;

    /// <summary>
    /// Событие изменения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Вызывает событие PropertyChanged для указанного свойства.
    /// </summary>
    /// <param name="propertyName">Имя изменившегося свойства.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Переопределение метода ToString для удобного отображения.
    /// </summary>
    public override string ToString() => $"{ProductName} × {Quantity} = {TotalPrice:C}";
}
