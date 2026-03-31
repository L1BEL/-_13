using System;
using System.IO;
using System.Windows;
using ClothesAccessoriesApp.Data;
using ClothesAccessoriesApp.Repositories;
using ClothesAccessoriesApp.Services;
using ClothesAccessoriesApp.ViewModels;

namespace ClothesAccessoriesApp;

public partial class App : Application
{
    private AppDbContext? _dbContext;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClothesAccessoriesApp",
            "catalog.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _dbContext = new AppDbContext(dbPath);
        DbInitializer.Initialize(_dbContext);

        var mainViewModel = new MainViewModel(
            new ProductRepository(_dbContext),
            _dbContext,
            new CartService(),
            new ThemeService(this));

        var window = new MainWindow
        {
            DataContext = mainViewModel
        };

        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _dbContext?.Dispose();
        base.OnExit(e);
    }
}
