using System;
using System.Linq;
using System.Windows;

namespace ClothesAccessoriesApp.Services;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeService
{
    private readonly Application _application;

    public ThemeService(Application application)
    {
        _application = application;
    }

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public void ApplyTheme(AppTheme theme)
    {
        var dictionaries = _application.Resources.MergedDictionaries;
        var existingTheme = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source is not null &&
            dictionary.Source.OriginalString.Contains("Theme.", StringComparison.OrdinalIgnoreCase) &&
            !dictionary.Source.OriginalString.EndsWith("Theme.xaml", StringComparison.OrdinalIgnoreCase));

        if (existingTheme is not null)
        {
            dictionaries.Remove(existingTheme);
        }

        var source = theme == AppTheme.Dark ? "Styles/Theme.Dark.xaml" : "Styles/Theme.Light.xaml";
        dictionaries.Insert(0, new ResourceDictionary { Source = new Uri(source, UriKind.Relative) });
        CurrentTheme = theme;
    }
}
