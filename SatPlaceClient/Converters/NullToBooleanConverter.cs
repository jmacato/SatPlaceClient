using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SatPlaceClient.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool res = false;
            if (value is null) res = true; else res = false;

            if ((parameter as string).ToLowerInvariant() == "invert")
                res = !res;

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}