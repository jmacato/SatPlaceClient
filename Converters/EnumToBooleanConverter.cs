using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SatPlaceClient.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                return value.ToString()
                            .Trim()
                            .Equals(parameter.ToString()
                                             .Trim(), 
                                    StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}