// Core/Converters/BoolToColorConverter.cs
using System.Globalization;

namespace LevelUp.Mobile.Core.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.Transparent;
        public Color FalseColor { get; set; } = Colors.Transparent;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? TrueColor : FalseColor;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}