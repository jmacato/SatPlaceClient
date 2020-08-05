using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SatPlaceClient.Exceptions;
using SatPlaceClient.Models;
using System;
using System.Globalization;

namespace SatPlaceClient.Converters
{
    public class PixelArrayToImageBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            if (value is GenericBitmap bitmap)
            {
                var newBitmap = new WriteableBitmap(new PixelSize(bitmap.Width, bitmap.Height), Vector.One, PixelFormat.Bgra8888);
                using (var lockedBitmap = newBitmap.Lock())
                {
                    unsafe
                    {
                        var TotalCanvasBytes = bitmap.Width * bitmap.Height * sizeof(GenericColor);
                        fixed (void* src = &bitmap.Pixels[0])
                            Buffer.MemoryCopy(src, lockedBitmap.Address.ToPointer(), TotalCanvasBytes, TotalCanvasBytes);
                    }
                }

                IBrush x = null;

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    x = new Avalonia.Media.ImageBrush(newBitmap).ToImmutable();
                }).Wait();
                     
                return x;
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