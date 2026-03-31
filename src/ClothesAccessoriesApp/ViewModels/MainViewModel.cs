using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Models;
using ClothesAccessoriesApp.Repositories;
using ClothesAccessoriesApp.Services;
using Microsoft.EntityFrameworkCore;

namespace ClothesAccessoriesApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IProductRepository _productRepository;
    private readonly AppDbContext _dbContext;
    private readonly CartService _cartService;
    private readonly ThemeService _themeService;

    private string _searchText = string.Empty;
    private Category? _selectedCategoryFilter;
    private Product? _selectedProduct;
    private UserRole _currentRole = UserRole.Guest;
    private string _newCategoryName = string.Empty;
    private string _newBrandName = string.Empty;
    private string _newMaterialName = string.Empty;

    public MainViewModel(IProductRepository productRepository, AppDbContext dbContext, CartService cartService, ThemeService themeService)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
        _cartService = cartService;
        _themeService = themeService;

        ProductEditor = new ProductEditorViewModel();
        FilteredProducts = [];
        Categories = [];
        Brands = [];
        Materials = [];
        CartItems = cartService.Items;

        SwitchRoleCommand = new RelayCommand(parameter => SwitchRole(parameter?.ToString()));
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        AddToCartCommand = new RelayCommand(AddToCart, () => SelectedProduct is not null);
        NewProductCommand = new RelayCommand(CreateNewProduct);
        SaveProductCommand = new RelayCommand(async () => await SaveProductAsync());
        DeleteProductCommand = new RelayCommand(async () => await DeleteSelectedProductAsync(), () => SelectedProduct is not null && IsAdminMode);
        AddCategoryCommand = new RelayCommand(async () => await AddCategoryAsync());
        AddBrandCommand = new RelayCommand(async () => await AddBrandAsync());
        AddMaterialCommand = new RelayCommand(async () => await AddMaterialAsync());

        _cartService.Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(CartTotal));
        _cartService.CartChanged += (_, _) => OnPropertyChanged(nameof(CartTotal));

        _themeService.ApplyTheme(AppTheme.Dark);
        LoadAsync().GetAwaiter().GetResult();
    }

    public ObservableCollection<Product> FilteredProducts { get; }
    public ObservableCollection<Category> Categories { get; }
    public ObservableCollection<Brand> Brands { get; }
    public ObservableCollection<Material> Materials { get; }
    public ObservableCollection<CartItem> CartItems { get; }
    public ProductEditorViewModel ProductEditor { get; }
    public RelayCommand SwitchRoleCommand { get; }
    public RelayCommand ToggleThemeCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand AddToCartCommand { get; }
    public RelayCommand NewProductCommand { get; }
    public RelayCommand SaveProductCommand { get; }
    public RelayCommand DeleteProductCommand { get; }
    public RelayCommand AddCategoryCommand { get; }
    public RelayCommand AddBrandCommand { get; }
    public RelayCommand AddMaterialCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilters();
            }
        }
    }

    public Category? SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set
        {
            if (SetProperty(ref _selectedCategoryFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                if (IsAdminMode)
                {
                    ProductEditor.Load(value);
                }

                AddToCartCommand.RaiseCanExecuteChanged();
                DeleteProductCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string NewCategoryName
    {
        get => _newCategoryName;
        set => SetProperty(ref _newCategoryName, value);
    }

    public string NewBrandName
    {
        get => _newBrandName;
        set => SetProperty(ref _newBrandName, value);
    }

    public string NewMaterialName
    {
        get => _newMaterialName;
        set => SetProperty(ref _newMaterialName, value);
    }

    public string SummaryText => $"Позиции: {FilteredProducts.Count} • Режим: {(IsAdminMode ? "Администратор" : "Гость")}";
    public decimal CartTotal => _cartService.GetTotal();
    public bool IsGuestMode => _currentRole == UserRole.Guest;
    public bool IsAdminMode => _currentRole == UserRole.Admin;
    public string ThemeButtonText => _themeService.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Темная тема";

    private async Task LoadAsync()
    {
        await LoadLookupsAsync();
        await LoadProductsAsync();
        SelectedCategoryFilter = Categories.FirstOrDefault();
        ClearFilters();
        CreateNewProduct();
    }

    private async Task LoadLookupsAsync()
    {
        Categories.Clear();
        Brands.Clear();
        Materials.Clear();

        Categories.Add(new Category { Id = 0, Name = "Все категории" });

        foreach (var category in await _dbContext.Categories.OrderBy(item => item.Name).ToListAsync())
        {
            Categories.Add(category);
        }

        foreach (var brand in await _dbContext.Brands.OrderBy(item => item.Name).ToListAsync())
        {
            Brands.Add(brand);
        }

        foreach (var material in await _dbContext.Materials.OrderBy(item => item.Name).ToListAsync())
        {
            Materials.Add(material);
        }
    }

    private async Task LoadProductsAsync()
    {
        var products = await _productRepository.GetCatalogAsync();

        FilteredProducts.Clear();
        foreach (var product in products)
        {
            FilteredProducts.Add(product);
        }

        SelectedProduct ??= FilteredProducts.FirstOrDefault();
        OnPropertyChanged(nameof(SummaryText));
    }

    private async void ApplyFilters()
    {
        var products = await _productRepository.GetCatalogAsync();
        var filtered = products.Where(product =>
            (string.IsNullOrWhiteSpace(SearchText) ||
             product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
             product.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
            (SelectedCategoryFilter is null ||
             SelectedCategoryFilter.Id == 0 ||
             product.CategoryId == SelectedCategoryFilter.Id));

        FilteredProducts.Clear();
        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }

        SelectedProduct = FilteredProducts.FirstOrDefault();
        OnPropertyChanged(nameof(SummaryText));
    }

    private void SwitchRole(string? role)
    {
        _currentRole = string.Equals(role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase)
            ? UserRole.Admin
            : UserRole.Guest;

        if (IsAdminMode)
        {
            ProductEditor.Load(SelectedProduct);
        }

        OnPropertyChanged(nameof(IsGuestMode));
        OnPropertyChanged(nameof(IsAdminMode));
        OnPropertyChanged(nameof(SummaryText));
        DeleteProductCommand.RaiseCanExecuteChanged();
    }

    private void ToggleTheme()
    {
        var nextTheme = _themeService.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        _themeService.ApplyTheme(nextTheme);
        OnPropertyChanged(nameof(ThemeButtonText));
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedCategoryFilter = Categories.FirstOrDefault();
        ApplyFilters();
    }

    private void AddToCart()
    {
        if (SelectedProduct is null)
        {
            return;
        }

        _cartService.Add(SelectedProduct);
        OnPropertyChanged(nameof(CartTotal));
    }

    private void CreateNewProduct()
    {
        ProductEditor.Load(null);
        ProductEditor.SelectedCategory = Categories.FirstOrDefault(category => category.Id != 0);
        ProductEditor.SelectedBrand = Brands.FirstOrDefault();
        ProductEditor.SelectedMaterial = Materials.FirstOrDefault();
    }

    private async Task SaveProductAsync()
    {
        if (!IsAdminMode || !ProductEditor.Validate())
        {
            return;
        }

        var product = ProductEditor.BuildProduct();

        if (product.Id == 0)
        {
            await _productRepository.AddAsync(product);
        }
        else
        {
            await _productRepository.UpdateAsync(product);
        }

        await LoadProductsAsync();
        ApplyFilters();
        SelectedProduct = FilteredProducts.FirstOrDefault(item => item.Name == product.Name && item.Price == product.Price);
    }

    private async Task DeleteSelectedProductAsync()
    {
        if (!IsAdminMode || SelectedProduct is null)
        {
            return;
        }

        var entity = await _dbContext.Products.FindAsync(SelectedProduct.Id);
        if (entity is null)
        {
            return;
        }

        await _productRepository.DeleteAsync(entity);
        await LoadProductsAsync();
        ApplyFilters();
        CreateNewProduct();
    }

    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            return;
        }

        _dbContext.Categories.Add(new Category { Name = NewCategoryName.Trim() });
        await _dbContext.SaveChangesAsync();
        NewCategoryName = string.Empty;
        await LoadLookupsAsync();
    }

    private async Task AddBrandAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBrandName))
        {
            return;
        }

        _dbContext.Brands.Add(new Brand { Name = NewBrandName.Trim() });
        await _dbContext.SaveChangesAsync();
        NewBrandName = string.Empty;
        await LoadLookupsAsync();
    }

    private async Task AddMaterialAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMaterialName))
        {
            return;
        }

        _dbContext.Materials.Add(new Material { Name = NewMaterialName.Trim() });
        await _dbContext.SaveChangesAsync();
        NewMaterialName = string.Empty;
        await LoadLookupsAsync();
    }
}


