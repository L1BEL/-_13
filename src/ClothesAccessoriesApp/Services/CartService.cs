using System;
using System.Collections.ObjectModel;
using System.Linq;
using ClothesAccessoriesApp.Models;

namespace ClothesAccessoriesApp.Services;

public class CartService
{
    public event EventHandler? CartChanged;

    public ObservableCollection<CartItem> Items { get; } = [];

    public void Add(Product product)
    {
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

            CartChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        existing.Quantity++;
        CartChanged?.Invoke(this, EventArgs.Empty);
    }

    public decimal GetTotal() => Items.Sum(item => item.TotalPrice);
}
