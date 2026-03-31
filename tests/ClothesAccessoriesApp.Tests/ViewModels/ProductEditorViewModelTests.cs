using ClothesAccessoriesApp.ViewModels;

namespace ClothesAccessoriesApp.Tests.ViewModels;

public class ProductEditorViewModelTests
{
    [Fact]
    public void Validate_EmptyName_ReturnsErrors()
    {
        var viewModel = new ProductEditorViewModel
        {
            Name = string.Empty,
            PriceText = "1200",
            StockQuantityText = "4"
        };

        var isValid = viewModel.Validate();

        Assert.False(isValid);
        Assert.True(viewModel.HasErrors);
    }
}
