using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ClothesAccessoriesApp.ViewModels;

/// <summary>
/// Базовый класс для ViewModel с поддержкой валидации через INotifyDataErrorInfo.
/// </summary>
public abstract class ValidatableViewModelBase : ViewModelBase, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = [];

    /// <summary>
    /// Возвращает true, если есть ошибки валидации.
    /// </summary>
    public bool HasErrors => _errors.Count != 0;

    /// <summary>
    /// Событие, возникающее при изменении ошибок валидации.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Получает ошибки валидации для указанного свойства или все ошибки.
    /// </summary>
    /// <param name="propertyName">Имя свойства или null для получения всех ошибок.</param>
    /// <returns>Коллекция сообщений об ошибках.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return _errors.SelectMany(item => item.Value);
        }

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Устанавливает ошибки для указанного свойства.
    /// </summary>
    /// <param name="propertyName">Имя свойства.</param>
    /// <param name="errors">Массив сообщений об ошибках.</param>
    protected void SetErrors(string propertyName, params string[] errors)
    {
        if (errors.Length == 0)
        {
            ClearErrors(propertyName);
            return;
        }

        _errors[propertyName] = errors.ToList();
        OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Очищает ошибки для указанного свойства.
    /// </summary>
    /// <param name="propertyName">Имя свойства.</param>
    protected void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Вызывает событие ErrorsChanged и обновляет свойство HasErrors.
    /// </summary>
    /// <param name="propertyName">Имя свойства, для которого изменились ошибки.</param>
    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }
}
