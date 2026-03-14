using System.Globalization;

namespace LevelUp.Mobile.Core.Converters
{
    /// <summary>
    /// string no nula Y no vacía → true
    /// Reemplaza IsNotNullConverter para Labels con texto opcional
    /// </summary>
    public class StringNotNullOrEmptyBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value as string);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
