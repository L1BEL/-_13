using System;
using System.IO;
using System.Windows;
using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Repositories;
using ClothesAccessoriesApp.Services;
using ClothesAccessoriesApp.ViewModels;

namespace ClothesAccessoriesApp;

/// <summary>
/// Класс приложения WPF, отвечающий за инициализацию и жизненный цикл.
/// </summary>
public partial class App : Application
{
    private AppDbContext? _dbContext;
    private CartService? _cartService;
    private ThemeService? _themeService;
    private MainViewModel? _mainViewModel;

    /// <summary>
    /// Вызывается при запуске приложения. Инициализирует базу данных и создаёт главное окно.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Настройка пути к базе данных SQLite
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClothesAccessoriesApp",
                "catalog.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            // Инициализация контекста базы данных
            _dbContext = new AppDbContext(dbPath);
            DbInitializer.Initialize(_dbContext);

            // Создание сервисов
            _cartService = new CartService();
            _themeService = new ThemeService(this);

            // Создание главной ViewModel с внедрением зависимостей
            _mainViewModel = new MainViewModel(
                new ProductRepository(_dbContext),
                _dbContext,
                _cartService,
                _themeService);

            // Создание главного окна (пока скрыто)
            var mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            MainWindow = mainWindow;

            // Открытие окна авторизации
            var loginWindow = new LoginWindow(_mainViewModel);
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка при запуске приложения: {ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    /// <summary>
    /// Вызывается при выходе из приложения. Освобождает ресурсы.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        // Освобождение ресурсов в правильном порядке
        _mainViewModel?.Dispose();
        _cartService?.Dispose();
        _dbContext?.Dispose();

        base.OnExit(e);
    }
}
