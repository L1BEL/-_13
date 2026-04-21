using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ClothesAccessoriesApp.Converters;

/// <summary>
/// Конвертер для преобразования пути к изображению в BitmapImage.
/// </summary>
public class ImagePathConverter : IValueConverter
{
    /// <summary>
    /// Преобразует путь к файлу изображения в BitmapImage.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            // Проверяем, является ли путь абсолютным
            var fullPath = Path.IsPathRooted(path) 
                ? path 
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.DecodePixelWidth = 100;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Обратное преобразование не поддерживается.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
