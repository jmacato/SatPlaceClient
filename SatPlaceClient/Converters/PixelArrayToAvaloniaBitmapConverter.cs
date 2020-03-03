using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SatPlaceClient.Exceptions;
using SatPlaceClient.Models;
using System;
using System.Globalization;

namespace SatPlaceClient.Converters
{
    public class PixelArrayToAvaloniaBitmapConverter : IValueConverter
    {
        private const int TotalCanvasBytes = 1000 * 1000 * 4;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            if (value is GenericPixel[] bitmap)
            {
                var newBitmap = new WriteableBitmap(new PixelSize(1000, 1000), Vector.One, PixelFormat.Bgra8888);

                using (var lockedBitmap = newBitmap.Lock())
                {
                    unsafe
                    {
                        fixed (void* src = &bitmap[0])
                            Buffer.MemoryCopy(src, lockedBitmap.Address.ToPointer(), TotalCanvasBytes, TotalCanvasBytes);
                    }
                }

                return newBitmap;
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