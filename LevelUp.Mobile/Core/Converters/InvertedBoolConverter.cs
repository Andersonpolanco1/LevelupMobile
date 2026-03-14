using System.Globalization;

namespace LevelUp.Mobile.Core.Converters
{
    /// <summary>
    /// true → false, false → true
    /// Úsalo con IsVisible="{Binding IsBusy, Converter={StaticResource InvertedBoolConverter}}"
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && !b;
    }
}
