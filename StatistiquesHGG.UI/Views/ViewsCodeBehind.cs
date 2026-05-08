// Converters used across views — partial classes are in their own .axaml.cs files

using Avalonia.Data.Converters;
using System.Globalization;

namespace StatistiquesHGG.UI;

public class DateTimeNowConverter : IValueConverter
{
    public static readonly DateTimeNowConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToColorConverter : IMultiValueConverter
{
    public static readonly BoolToColorConverter Instance = new();
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isSuccess)
            return isSuccess ? Avalonia.Media.Brushes.LightGreen : Avalonia.Media.Brushes.LightCoral;
        return Avalonia.Media.Brushes.Transparent;
    }
}

public class StatutToColorConverter : IValueConverter
{
    public static readonly StatutToColorConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Validee" or "True" => Avalonia.Media.Brush.Parse("#D1FAE5"),
            "Rejetee" or "False" => Avalonia.Media.Brush.Parse("#FEE2E2"),
            _ => Avalonia.Media.Brush.Parse("#FEF3C7")
        };
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NiveauToColorConverter : IValueConverter
{
    public static readonly NiveauToColorConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Excellent" => Avalonia.Media.Brush.Parse("#D1FAE5"),
            "Bon" => Avalonia.Media.Brush.Parse("#DBEAFE"),
            "Moyen" => Avalonia.Media.Brush.Parse("#FEF3C7"),
            "AAmeliorer" => Avalonia.Media.Brush.Parse("#FEE2E2"),
            _ => Avalonia.Media.Brush.Parse("#F1F5F9")
        };
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NiveauToForegroundConverter : IValueConverter
{
    public static readonly NiveauToForegroundConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Excellent" => Avalonia.Media.Brush.Parse("#059669"),
            "Bon" => Avalonia.Media.Brush.Parse("#1D4ED8"),
            "Moyen" => Avalonia.Media.Brush.Parse("#D97706"),
            "AAmeliorer" => Avalonia.Media.Brush.Parse("#DC2626"),
            _ => Avalonia.Media.Brush.Parse("#64748B")
        };
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
