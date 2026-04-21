using System;
using System.Windows;
using ClothesAccessoriesApp.ViewModels;

namespace ClothesAccessoriesApp;

/// <summary>
/// Окно авторизации для выбора роли пользователя.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly LoginViewModel _viewModel;

    public LoginWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _viewModel = new LoginViewModel(this, _mainViewModel);
        DataContext = _viewModel;
        
        // Закрываем главное окно при закрытии окна авторизации (если не вошли)
        this.Closed += (s, e) =>
        {
            if (!_mainViewModel.IsLoggedIn)
            {
                Application.Current.Shutdown();
            }
        };
    }
}
