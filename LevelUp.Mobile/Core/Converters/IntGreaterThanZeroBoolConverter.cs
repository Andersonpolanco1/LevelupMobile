// Core/Converters/IntGreaterThanZeroBoolConverter.cs
using System.Globalization;

namespace LevelUp.Mobile.Core.Converters;

public class IntGreaterThanZeroBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int n && n > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}