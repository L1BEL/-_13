using System;
using System.Windows;
using System.Windows.Input;

namespace ClothesAccessoriesApp.ViewModels;

/// <summary>
/// ViewModel для окна авторизации и выбора роли пользователя.
/// </summary>
public class LoginViewModel : ViewModelBase
{
    private readonly Window _window;
    private readonly MainViewModel _mainViewModel;

    public LoginViewModel(Window window, MainViewModel mainViewModel)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

        LoginAsGuestCommand = new RelayCommand(LoginAsGuest);
        LoginAsEmployeeCommand = new RelayCommand(LoginAsEmployee);
        LoginAsAdminCommand = new RelayCommand(LoginAsAdmin);
        CancelCommand = new RelayCommand(Cancel);
    }

    /// <summary>
    /// Команда входа в качестве гостя.
    /// </summary>
    public ICommand LoginAsGuestCommand { get; }

    /// <summary>
    /// Команда входа в качестве работника.
    /// </summary>
    public ICommand LoginAsEmployeeCommand { get; }

    /// <summary>
    /// Команда входа в качестве администратора.
    /// </summary>
    public ICommand LoginAsAdminCommand { get; }

    /// <summary>
    /// Команда отмены входа.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Выполняет вход в режиме гостя.
    /// </summary>
    private void LoginAsGuest()
    {
        _mainViewModel.SwitchRole("Guest");
        CloseWindow();
    }

    /// <summary>
    /// Выполняет вход в режиме работника.
    /// </summary>
    private void LoginAsEmployee()
    {
        _mainViewModel.SwitchRole("Employee");
        CloseWindow();
    }

    /// <summary>
    /// Выполняет вход в режиме администратора.
    /// </summary>
    private void LoginAsAdmin()
    {
        _mainViewModel.SwitchRole("Admin");
        CloseWindow();
    }

    /// <summary>
    /// Отменяет вход и закрывает окно.
    /// </summary>
    private void Cancel()
    {
        CloseWindow();
    }

    /// <summary>
    /// Закрывает окно авторизации.
    /// </summary>
    private void CloseWindow()
    {
        if (_window.IsVisible)
        {
            _window.Close();
        }
    }
}
