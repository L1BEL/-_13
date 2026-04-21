using System;
using System.Windows.Input;

namespace ClothesAccessoriesApp.ViewModels;

/// <summary>
/// Реализация ICommand для выполнения действий в ViewModel.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Инициализирует новый экземпляр RelayCommand без параметра.
    /// </summary>
    /// <param name="execute">Действие для выполнения.</param>
    /// <param name="canExecute">Функция проверки возможности выполнения (необязательно).</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если execute равен null.</exception>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute())
    {
        if (execute is null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
    }

    /// <summary>
    /// Инициализирует новый экземпляр RelayCommand с параметром.
    /// </summary>
    /// <param name="execute">Действие для выполнения, принимающее параметр.</param>
    /// <param name="canExecute">Функция проверки возможности выполнения с параметром (необязательно).</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если execute равен null.</exception>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Событие, возникающее при изменении состояния выполнения команды.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Определяет, может ли команда быть выполнена в текущем состоянии.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>true, если команда может быть выполнена; иначе false.</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Выполняет команду.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// Вызывает событие CanExecuteChanged для обновления состояния кнопок UI.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
