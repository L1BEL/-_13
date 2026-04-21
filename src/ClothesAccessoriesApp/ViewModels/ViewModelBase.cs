using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClothesAccessoriesApp.ViewModels;

/// <summary>
/// Базовый класс для ViewModel с реализацией INotifyPropertyChanged.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Событие, возникающее при изменении свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Устанавливает значение свойства и вызывает событие PropertyChanged, если значение изменилось.
    /// </summary>
    /// <typeparam name="T">Тип свойства.</typeparam>
    /// <param name="storage">Ссылка на поле хранения.</param>
    /// <param name="value">Новое значение.</param>
    /// <param name="propertyName">Имя свойства (заполняется автоматически).</param>
    /// <returns>true, если значение было изменено; иначе false.</returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Вызывает событие PropertyChanged для указанного свойства.
    /// </summary>
    /// <param name="propertyName">Имя свойства (заполняется автоматически).</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
