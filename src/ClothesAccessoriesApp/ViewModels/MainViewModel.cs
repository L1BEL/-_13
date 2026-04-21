using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Models;
using ClothesAccessoriesApp.Repositories;
using ClothesAccessoriesApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace ClothesAccessoriesApp.ViewModels;

/// <summary>
/// Основная ViewModel приложения, управляющая каталогом товаров, корзиной и фильтрацией.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IProductRepository _productRepository;
    private readonly AppDbContext _dbContext;
    private readonly CartService _cartService;
    private readonly ThemeService _themeService;
    private bool _isDisposed;

    private string _searchText = string.Empty;
    private Category? _selectedCategoryFilter;
    private Product? _selectedProduct;
    private UserRole _currentRole = UserRole.Guest;
    private string _newCategoryName = string.Empty;
    private string _newBrandName = string.Empty;
    private string _newMaterialName = string.Empty;
    private bool _isLoading = true;
    private bool _isLoginWindowOpen;
    private string _sortBy = "Name";
    private bool _sortAscending = true;
    private BitmapImage? _selectedProductImage;
    private string _imageUploadError = string.Empty;

    public MainViewModel(
        IProductRepository productRepository,
        AppDbContext dbContext,
        CartService cartService,
        ThemeService themeService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));

        ProductEditor = new ProductEditorViewModel();
        FilteredProducts = [];
        Categories = [];
        Brands = [];
        Materials = [];
        CartItems = cartService.Items;

        // Инициализация команд
        OpenLoginWindowCommand = new RelayCommand(OpenLoginWindow);
        SwitchRoleCommand = new RelayCommand(parameter => SwitchRole(parameter?.ToString()), () => !_isLoginWindowOpen);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        AddToCartCommand = new RelayCommand(AddToCart, CanAddToCart);
        NewProductCommand = new RelayCommand(CreateNewProduct, CanEditProducts);
        SaveProductCommand = new RelayCommand(async () => await SaveProductAsync(), CanSaveProduct);
        DeleteProductCommand = new RelayCommand(async () => await DeleteSelectedProductAsync(), CanDeleteProduct);
        AddCategoryCommand = new RelayCommand(async () => await AddCategoryAsync(), CanAddCategory);
        AddBrandCommand = new RelayCommand(async () => await AddBrandAsync(), CanAddBrand);
        AddMaterialCommand = new RelayCommand(async () => await AddMaterialAsync(), CanAddMaterial);
        UploadImageCommand = new RelayCommand(UploadImage, () => IsAdminMode || IsEmployeeMode);
        SortByNameCommand = new RelayCommand(() => SortProducts("Name"));
        SortByPriceCommand = new RelayCommand(() => SortProducts("Price"));
        RemoveFromCartCommand = new RelayCommand(RemoveFromCart, () => SelectedCartItem is not null);
        ClearCartCommand = new RelayCommand(ClearCart, () => CartItems.Any());

        // Подписка на события корзины с защитой от утечек памяти
        _cartService.Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(CartTotal));
        _cartService.CartChanged += (_, _) => OnPropertyChanged(nameof(CartTotal));

        _themeService.ApplyTheme(AppTheme.Light);
        
        // Асинхронная загрузка без блокировки UI
        _ = InitializeAsync();
    }

    public ObservableCollection<Product> FilteredProducts { get; }
    public ObservableCollection<Category> Categories { get; }
    public ObservableCollection<Brand> Brands { get; }
    public ObservableCollection<Material> Materials { get; }
    public ObservableCollection<CartItem> CartItems { get; }
    public ProductEditorViewModel ProductEditor { get; }

    public RelayCommand OpenLoginWindowCommand { get; }
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
    public RelayCommand UploadImageCommand { get; }
    public RelayCommand SortByNameCommand { get; }
    public RelayCommand SortByPriceCommand { get; }
    public RelayCommand RemoveFromCartCommand { get; }
    public RelayCommand ClearCartCommand { get; }

    /// <summary>
    /// Текст для поиска товаров по названию или описанию.
    /// </summary>
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

    /// <summary>
    /// Выбранная категория для фильтрации.
    /// </summary>
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

    /// <summary>
    /// Выбранный товар в списке.
    /// </summary>
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                if (IsAdminMode && value is not null)
                {
                    ProductEditor.Load(value);
                }

                AddToCartCommand.RaiseCanExecuteChanged();
                DeleteProductCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Название новой категории для добавления.
    /// </summary>
    public string NewCategoryName
    {
        get => _newCategoryName;
        set
        {
            if (SetProperty(ref _newCategoryName, value))
            {
                AddCategoryCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Название нового бренда для добавления.
    /// </summary>
    public string NewBrandName
    {
        get => _newBrandName;
        set
        {
            if (SetProperty(ref _newBrandName, value))
            {
                AddBrandCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Название нового материала для добавления.
    /// </summary>
    public string NewMaterialName
    {
        get => _newMaterialName;
        set
        {
            if (SetProperty(ref _newMaterialName, value))
            {
                AddMaterialCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Индикатор загрузки данных.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Сводная информация о текущем состоянии (количество товаров и режим).
    /// </summary>
    public string SummaryText => $"Позиции: {FilteredProducts.Count} • Режим: {GetCurrentRoleName()}";

    /// <summary>
    /// Общая сумма товаров в корзине.
    /// </summary>
    public decimal CartTotal => _cartService.GetTotal();

    /// <summary>
    /// Флаг, указывающий, что приложение работает в режиме гостя.
    /// </summary>
    public bool IsGuestMode => _currentRole == UserRole.Guest;

    /// <summary>
    /// Флаг, указывающий, что приложение работает в режиме работника.
    /// </summary>
    public bool IsEmployeeMode => _currentRole == UserRole.Employee;

    /// <summary>
    /// Флаг, указывающий, что приложение работает в режиме администратора.
    /// </summary>
    public bool IsAdminMode => _currentRole == UserRole.Admin;

    /// <summary>
    /// Флаг, указывающий, открыто ли окно авторизации.
    /// </summary>
    public bool IsLoginWindowOpen
    {
        get => _isLoginWindowOpen;
        private set
        {
            if (SetProperty(ref _isLoginWindowOpen, value))
            {
                SwitchRoleCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Текст кнопки переключения темы.
    /// </summary>
    public string ThemeButtonText => _themeService.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Темная тема";

    /// <summary>
    /// Текущий способ сортировки товаров.
    /// </summary>
    public string SortBy
    {
        get => _sortBy;
        private set => SetProperty(ref _sortBy, value);
    }

    /// <summary>
    /// Направление сортировки (по возрастанию или убыванию).
    /// </summary>
    public bool SortAscending
    {
        get => _sortAscending;
        private set => SetProperty(ref _sortAscending, value);
    }

    /// <summary>
    /// Изображение выбранного товара.
    /// </summary>
    public BitmapImage? SelectedProductImage
    {
        get => _selectedProductImage;
        private set => SetProperty(ref _selectedProductImage, value);
    }

    /// <summary>
    /// Сообщение об ошибке при загрузке изображения.
    /// </summary>
    public string ImageUploadError
    {
        get => _imageUploadError;
        private set => SetProperty(ref _imageUploadError, value);
    }

    /// <summary>
    /// Выбранный элемент корзины для удаления.
    /// </summary>
    public CartItem? SelectedCartItem { get; private set; }

    /// <summary>
    /// Возвращает название текущей роли пользователя.
    /// </summary>
    private string GetCurrentRoleName() => _currentRole switch
    {
        UserRole.Guest => "Гость",
        UserRole.Employee => "Работник",
        UserRole.Admin => "Администратор",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Инициализирует приложение: загружает справочники и товары.
    /// </summary>
    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            await LoadLookupsAsync();
            await LoadProductsAsync();
            SelectedCategoryFilter = Categories.FirstOrDefault();
            ClearFilters();
            CreateNewProduct();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Загружает справочники (категории, бренды, материалы) из базы данных.
    /// </summary>
    private async Task LoadLookupsAsync()
    {
        Categories.Clear();
        Brands.Clear();
        Materials.Clear();

        // Добавляем категорию "Все категории" для фильтра
        Categories.Add(new Category { Id = 0, Name = "Все категории" });

        var categories = await _dbContext.Categories.OrderBy(item => item.Name).ToListAsync();
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        var brands = await _dbContext.Brands.OrderBy(item => item.Name).ToListAsync();
        foreach (var brand in brands)
        {
            Brands.Add(brand);
        }

        var materials = await _dbContext.Materials.OrderBy(item => item.Name).ToListAsync();
        foreach (var material in materials)
        {
            Materials.Add(material);
        }
    }

    /// <summary>
    /// Загружает все товары из базы данных.
    /// </summary>
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

    /// <summary>
    /// Применяет фильтры к списку товаров на основе поискового запроса и выбранной категории.
    /// </summary>
    private void ApplyFilters()
    {
        var products = _productRepository.GetCatalogAsync().Result;
        var filtered = products.Where(product =>
            (string.IsNullOrWhiteSpace(SearchText) ||
             product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
             product.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
            (SelectedCategoryFilter is null ||
             SelectedCategoryFilter.Id == 0 ||
             product.CategoryId == SelectedCategoryFilter.Id));

        // Применяем сортировку
        filtered = SortAscending 
            ? filtered.OrderBy(p => GetSortProperty(p, SortBy))
            : filtered.OrderByDescending(p => GetSortProperty(p, SortBy));

        FilteredProducts.Clear();
        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }

        SelectedProduct = FilteredProducts.FirstOrDefault();
        LoadProductImage();
        OnPropertyChanged(nameof(SummaryText));
    }

    /// <summary>
    /// Получает значение свойства для сортировки.
    /// </summary>
    private IComparable GetSortProperty(Product product, string sortBy)
    {
        return sortBy switch
        {
            "Name" => product.Name,
            "Price" => product.Price,
            "Category" => product.Category?.Name ?? string.Empty,
            _ => product.Name
        };
    }

    /// <summary>
    /// Сортирует товары по указанному полю.
    /// </summary>
    private void SortProducts(string sortBy)
    {
        if (SortBy == sortBy)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortBy = sortBy;
            SortAscending = true;
        }

        ApplyFilters();
    }

    /// <summary>
    /// Переключает роль пользователя между гостем, работником и администратором.
    /// </summary>
    private void SwitchRole(string? role)
    {
        _currentRole = role switch
        {
            "Admin" => UserRole.Admin,
            "Employee" => UserRole.Employee,
            _ => UserRole.Guest
        };

        if (IsAdminMode && SelectedProduct is not null)
        {
            ProductEditor.Load(SelectedProduct);
        }
        else if (!IsAdminMode)
        {
            // При переключении на гостя или работника очищаем редактор
            CreateNewProduct();
        }

        OnPropertyChanged(nameof(IsGuestMode));
        OnPropertyChanged(nameof(IsEmployeeMode));
        OnPropertyChanged(nameof(IsAdminMode));
        OnPropertyChanged(nameof(SummaryText));
        DeleteProductCommand.RaiseCanExecuteChanged();
        SaveProductCommand.RaiseCanExecuteChanged();
        NewProductCommand.RaiseCanExecuteChanged();
        AddCategoryCommand.RaiseCanExecuteChanged();
        AddBrandCommand.RaiseCanExecuteChanged();
        AddMaterialCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Открывает окно авторизации для выбора роли.
    /// </summary>
    private void OpenLoginWindow()
    {
        IsLoginWindowOpen = true;
        
        var loginWindow = new LoginWindow(this)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        loginWindow.ShowDialog();
        IsLoginWindowOpen = false;
    }

    /// <summary>
    /// Переключает тему оформления между тёмной и светлой.
    /// </summary>
    private void ToggleTheme()
    {
        var nextTheme = _themeService.CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        _themeService.ApplyTheme(nextTheme);
        OnPropertyChanged(nameof(ThemeButtonText));
    }

    /// <summary>
    /// Сбрасывает все фильтры к значениям по умолчанию.
    /// </summary>
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedCategoryFilter = Categories.FirstOrDefault();
        ApplyFilters();
    }

    /// <summary>
    /// Добавляет выбранный товар в корзину.
    /// </summary>
    private void AddToCart()
    {
        if (SelectedProduct is null)
        {
            return;
        }

        _cartService.Add(SelectedProduct);
        OnPropertyChanged(nameof(CartTotal));
        RemoveFromCartCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Проверяет возможность добавления товара в корзину.
    /// </summary>
    private bool CanAddToCart() => SelectedProduct is not null && !IsLoading;

    /// <summary>
    /// Создаёт новый пустой товар для редактирования.
    /// </summary>
    private void CreateNewProduct()
    {
        ProductEditor.Load(null);
        ProductEditor.SelectedCategory = Categories.FirstOrDefault(category => category.Id != 0);
        ProductEditor.SelectedBrand = Brands.FirstOrDefault();
        ProductEditor.SelectedMaterial = Materials.FirstOrDefault();
    }

    /// <summary>
    /// Сохраняет текущий товар (добавляет или обновляет) в базу данных.
    /// </summary>
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

    /// <summary>
    /// Удаляет выбранный товар из базы данных с подтверждением.
    /// </summary>
    private async Task DeleteSelectedProductAsync()
    {
        if (!IsAdminMode || SelectedProduct is null)
        {
            return;
        }

        // Запрос подтверждения на удаление
        var result = MessageBox.Show(
            $"Вы уверены, что хотите удалить товар \"{SelectedProduct.Name}\"?\n\nЭто действие нельзя отменить.",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var entity = await _dbContext.Products.FindAsync(SelectedProduct.Id);
        if (entity is null)
        {
            MessageBox.Show("Товар не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            await _productRepository.DeleteAsync(entity);
            await LoadProductsAsync();
            ApplyFilters();
            CreateNewProduct();
            MessageBox.Show("Товар успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Добавляет новую категорию в базу данных.
    /// </summary>
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

    /// <summary>
    /// Добавляет новый бренд в базу данных.
    /// </summary>
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

    /// <summary>
    /// Добавляет новый материал в базу данных.
    /// </summary>
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

    #region CanExecute методы для команд

    /// <summary>
    /// Проверяет возможность редактирования товаров (доступно работнику и администратору).
    /// </summary>
    private bool CanEditProducts() => IsEmployeeMode || IsAdminMode;

    /// <summary>
    /// Проверяет возможность сохранения товара (доступно только администратору).
    /// </summary>
    private bool CanSaveProduct() => IsAdminMode && ProductEditor is not null;

    /// <summary>
    /// Проверяет возможность удаления выбранного товара (доступно только администратору).
    /// </summary>
    private bool CanDeleteProduct() => IsAdminMode && SelectedProduct is not null;

    /// <summary>
    /// Проверяет возможность добавления новой категории (доступно только администратору).
    /// </summary>
    private bool CanAddCategory() => IsAdminMode && !string.IsNullOrWhiteSpace(NewCategoryName);

    /// <summary>
    /// Проверяет возможность добавления нового бренда (доступно только администратору).
    /// </summary>
    private bool CanAddBrand() => IsAdminMode && !string.IsNullOrWhiteSpace(NewBrandName);

    /// <summary>
    /// Проверяет возможность добавления нового материала (доступно только администратору).
    /// </summary>
    private bool CanAddMaterial() => IsAdminMode && !string.IsNullOrWhiteSpace(NewMaterialName);

    #endregion

    #region Загрузка изображений и работа с корзиной

    /// <summary>
    /// Загружает изображение для выбранного товара через диалог выбора файла.
    /// </summary>
    private void UploadImage()
    {
        if (!IsAdminMode && !IsEmployeeMode)
        {
            return;
        }

        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*",
                Title = "Выберите изображение товара",
                CheckFileExists = true,
                CheckPathExists = true
            };

            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                var filePath = openFileDialog.FileName;
                
                // Проверяем размер файла (максимум 5 МБ)
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    ImageUploadError = "Размер файла не должен превышать 5 МБ.";
                    MessageBox.Show(ImageUploadError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Копируем изображение в папку Images приложения
                var imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(filePath)}";
                var destinationPath = Path.Combine(imagesFolder, fileName);
                
                File.Copy(filePath, destinationPath, true);

                // Сохраняем относительный путь
                var relativePath = Path.Combine("Images", fileName);
                ProductEditor.ImagePath = relativePath;
                
                // Загружаем изображение для предпросмотра
                LoadProductImage();
                
                ImageUploadError = string.Empty;
                MessageBox.Show("Изображение успешно загружено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            ImageUploadError = $"Ошибка при загрузке изображения: {ex.Message}";
            MessageBox.Show(ImageUploadError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Загружает изображение выбранного товара для отображения.
    /// </summary>
    private void LoadProductImage()
    {
        if (SelectedProduct is null || string.IsNullOrWhiteSpace(SelectedProduct.ImagePath))
        {
            SelectedProductImage = null;
            return;
        }

        try
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SelectedProduct.ImagePath);
            
            if (File.Exists(fullPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 300;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                SelectedProductImage = bitmap;
                ImageUploadError = string.Empty;
            }
            else
            {
                SelectedProductImage = null;
                ImageUploadError = "Файл изображения не найден.";
            }
        }
        catch (Exception ex)
        {
            SelectedProductImage = null;
            ImageUploadError = $"Ошибка загрузки изображения: {ex.Message}";
        }
    }

    /// <summary>
    /// Удаляет выбранный элемент из корзины.
    /// </summary>
    private void RemoveFromCart()
    {
        if (SelectedCartItem is null)
        {
            return;
        }

        _cartService.Remove(SelectedCartItem.ProductId);
        OnPropertyChanged(nameof(CartTotal));
        RemoveFromCartCommand.RaiseCanExecuteChanged();
        ClearCartCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Очищает корзину от всех товаров.
    /// </summary>
    private void ClearCart()
    {
        _cartService.Clear();
        OnPropertyChanged(nameof(CartTotal));
        RemoveFromCartCommand.RaiseCanExecuteChanged();
        ClearCartCommand.RaiseCanExecuteChanged();
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Освобождает ресурсы, используемые ViewModel.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        // Отписка от событий корзины для предотвращения утечек памяти
        _cartService.Items.CollectionChanged -= (_, _) => OnPropertyChanged(nameof(CartTotal));
        _cartService.CartChanged -= (_, _) => OnPropertyChanged(nameof(CartTotal));

        _isDisposed = true;
    }

    #endregion
}


