using Avalonia.Data.Converters;
using System.Globalization;

namespace StatistiquesHGG.UI;

public class NullToTextConverter : IValueConverter
{
    public static readonly NullToTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = parameter?.ToString()?.Split('|') ?? new[] { "Oui", "Non" };
        // value is null = new user, not null = edit existing
        return value == null ? (parts.Length > 1 ? parts[1] : "Nouveau") : parts[0];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NullToBoolConverter : IValueConverter
{
    public static readonly NullToBoolConverter Instance = new();

    // Returns true (enabled) when value IS null (new user, login can be typed)
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value == null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToStringConverter : IValueConverter
{
    public static readonly BoolToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            var parts = parameter?.ToString()?.Split('|') ?? new[] { "Oui", "Non" };
            return b ? parts[0] : (parts.Length > 1 ? parts[1] : "Non");
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
