using System.Globalization;

namespace LevelUp.Mobile.Core.Converters
{
    /// <summary>
    /// int == 0 → true  (para mostrar empty state cuando Days.Count == 0)
    /// </summary>
    public class IntEqualsZeroBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int i && i == 0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
