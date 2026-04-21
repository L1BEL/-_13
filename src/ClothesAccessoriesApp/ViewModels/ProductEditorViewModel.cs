using ClothesAccessoriesApp.Models;

namespace ClothesAccessoriesApp.ViewModels;

public class ProductEditorViewModel : ValidatableViewModelBase
{
    private int _id;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _priceText = string.Empty;
    private string _stockQuantityText = string.Empty;
    private string _imagePath = string.Empty;
    private Category? _selectedCategory;
    private Brand? _selectedBrand;
    private Material? _selectedMaterial;

    public int Id
    {
        get => _id;
        private set
        {
            if (SetProperty(ref _id, value))
            {
                OnPropertyChanged(nameof(IsEditMode));
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                ValidateName();
            }
        }
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string PriceText
    {
        get => _priceText;
        set
        {
            if (SetProperty(ref _priceText, value))
            {
                ValidatePrice();
            }
        }
    }

    public string StockQuantityText
    {
        get => _stockQuantityText;
        set
        {
            if (SetProperty(ref _stockQuantityText, value))
            {
                ValidateStockQuantity();
            }
        }
    }

    /// <summary>
    /// Путь к изображению товара.
    /// </summary>
    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                ValidateCategory();
            }
        }
    }

    public Brand? SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            if (SetProperty(ref _selectedBrand, value))
            {
                ValidateBrand();
            }
        }
    }

    public Material? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (SetProperty(ref _selectedMaterial, value))
            {
                ValidateMaterial();
            }
        }
    }

    public bool IsEditMode => Id != 0;

    public void Load(Product? product)
    {
        if (product is null)
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            PriceText = string.Empty;
            StockQuantityText = "0";
            ImagePath = string.Empty;
            SelectedCategory = null;
            SelectedBrand = null;
            SelectedMaterial = null;
            return;
        }

        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        PriceText = product.Price.ToString("F2");
        StockQuantityText = product.StockQuantity.ToString();
        ImagePath = product.ImagePath ?? string.Empty;
        SelectedCategory = product.Category;
        SelectedBrand = product.Brand;
        SelectedMaterial = product.Material;
    }

    public bool Validate()
    {
        ValidateName();
        ValidatePrice();
        ValidateStockQuantity();
        ValidateCategory();
        ValidateBrand();
        ValidateMaterial();
        return !HasErrors;
    }

    public Product BuildProduct()
    {
        return new Product
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description.Trim(),
            Price = decimal.Parse(PriceText),
            StockQuantity = int.Parse(StockQuantityText),
            CategoryId = SelectedCategory!.Id,
            BrandId = SelectedBrand!.Id,
            MaterialId = SelectedMaterial!.Id,
            ImagePath = string.IsNullOrWhiteSpace(ImagePath) ? null : ImagePath.Trim()
        };
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            SetErrors(nameof(Name), "Введите название товара.");
            return;
        }

        ClearErrors(nameof(Name));
    }

    private void ValidatePrice()
    {
        if (!decimal.TryParse(PriceText, out var price) || price <= 0)
        {
            SetErrors(nameof(PriceText), "Цена должна быть больше нуля.");
            return;
        }

        ClearErrors(nameof(PriceText));
    }

    private void ValidateStockQuantity()
    {
        if (!int.TryParse(StockQuantityText, out var stock) || stock < 0)
        {
            SetErrors(nameof(StockQuantityText), "Остаток должен быть целым числом не меньше нуля.");
            return;
        }

        ClearErrors(nameof(StockQuantityText));
    }

    private void ValidateCategory()
    {
        if (SelectedCategory is null)
        {
            SetErrors(nameof(SelectedCategory), "Выберите категорию.");
            return;
        }

        ClearErrors(nameof(SelectedCategory));
    }

    private void ValidateBrand()
    {
        if (SelectedBrand is null)
        {
            SetErrors(nameof(SelectedBrand), "Выберите бренд.");
            return;
        }

        ClearErrors(nameof(SelectedBrand));
    }

    private void ValidateMaterial()
    {
        if (SelectedMaterial is null)
        {
            SetErrors(nameof(SelectedMaterial), "Выберите материал.");
            return;
        }

        ClearErrors(nameof(SelectedMaterial));
    }
}
