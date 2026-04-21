using System;
using System.Collections.ObjectModel;
using System.Linq;
using ClothesAccessoriesApp.Models;

namespace ClothesAccessoriesApp.Services;

/// <summary>
/// Сервис для управления корзиной покупок.
/// </summary>
public class CartService : IDisposable
{
    private bool _isDisposed;

    /// <summary>
    /// Событие, возникающее при изменении состава корзины.
    /// </summary>
    public event EventHandler? CartChanged;

    /// <summary>
    /// Коллекция элементов корзины.
    /// </summary>
    public ObservableCollection<CartItem> Items { get; } = [];

    /// <summary>
    /// Добавляет товар в корзину или увеличивает количество, если товар уже есть.
    /// </summary>
    /// <param name="product">Товар для добавления.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если product равен null.</exception>
    public void Add(Product product)
    {
        if (product is null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        var existing = Items.FirstOrDefault(item => item.ProductId == product.Id);

        if (existing is null)
        {
            Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = 1
            });
        }
        else
        {
            existing.Quantity++;
        }

        OnCartChanged();
    }

    /// <summary>
    /// Удаляет товар из корзины по идентификатору.
    /// </summary>
    /// <param name="productId">Идентификатор товара для удаления.</param>
    /// <returns>true, если товар был найден и удалён; иначе false.</returns>
    public bool Remove(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return false;
        }

        Items.Remove(item);
        OnCartChanged();
        return true;
    }

    /// <summary>
    /// Очищает корзину от всех товаров.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        OnCartChanged();
    }

    /// <summary>
    /// Вычисляет общую сумму всех товаров в корзине.
    /// </summary>
    /// <returns>Общая сумма в виде десятичного числа.</returns>
    public decimal GetTotal() => Items.Sum(item => item.TotalPrice);

    /// <summary>
    /// Вызывает событие CartChanged.
    /// </summary>
    private void OnCartChanged() => CartChanged?.Invoke(this, EventArgs.Empty);

    #region IDisposable

    /// <summary>
    /// Освобождает ресурсы, используемые сервисом.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        // Очищаем подписчиков событий
        CartChanged = null;
        Items.Clear();

        _isDisposed = true;
    }

    #endregion
}
