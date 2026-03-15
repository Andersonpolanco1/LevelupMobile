using System.Globalization;

namespace LevelUp.Mobile.Core.Converters
{
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is null || (value is string s && string.IsNullOrEmpty(s));
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
