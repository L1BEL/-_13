using ClothesAccessoriesApp.Models;
using ClothesAccessoriesApp.Services;

namespace ClothesAccessoriesApp.Tests.Services;

public class CartServiceTests
{
    [Fact]
    public void Add_SameProductTwice_CalculatesTotal()
    {
        var service = new CartService();
        var product = new Product
        {
            Id = 1,
            Name = "Sprint Pro X",
            Price = 2500m
        };

        service.Add(product);
        service.Add(product);

        Assert.Single(service.Items);
        Assert.Equal(2, service.Items[0].Quantity);
        Assert.Equal(5000m, service.GetTotal());
    }
}
