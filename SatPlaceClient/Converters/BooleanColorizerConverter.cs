using Avalonia.Data.Converters;
using Avalonia.Media;
using SatPlaceClient.Exceptions;
using System;
using System.Globalization;

namespace SatPlaceClient.Converters
{
    public class BooleanColorizerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool on)
            {
                if (parameter is string str)
                {
                    var options = str.Split(':');
                    if (options.Length < 2)
                    {
                        throw new ArgumentException("Two options are required by the converter.", nameof(parameter));
                    }

                    var color1 = Color.Parse(options[0]);                    
                    var color2 = Color.Parse(options[1]);

                    return on ? color1 : color2;
                }
                else
                {
                    throw new TypeArgumentException(parameter, typeof(string), nameof(parameter));
                }
            }
            else
            {
                throw new TypeArgumentException(value, typeof(bool), nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}