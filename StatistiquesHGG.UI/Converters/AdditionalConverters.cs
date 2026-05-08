using Avalonia.Data.Converters;
using System.Globalization;

namespace StatistiquesHGG.UI;

public class RangToColorConverter : IMultiValueConverter
{
    public static readonly RangToColorConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is int rang)
        {
            return rang switch
            {
                1 => Avalonia.Media.Brush.Parse("#F59E0B"),
                2 => Avalonia.Media.Brush.Parse("#94A3B8"),
                3 => Avalonia.Media.Brush.Parse("#92400E"),
                _ => Avalonia.Media.Brush.Parse("#005BA1")
            };
        }
        return Avalonia.Media.Brush.Parse("#005BA1");
    }
}

public class RangLessThan4Converter : IValueConverter
{
    public static readonly RangLessThan4Converter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int rang && rang <= 3;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
