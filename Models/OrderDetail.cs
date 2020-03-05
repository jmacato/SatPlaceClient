using System.Numerics;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ReactiveUI;
using SatPlaceClient.Models.Json;

namespace SatPlaceClient.Models
{
    public class OrderDetail : ReactiveObject
    {
        public OrderDetail(GenericBitmap genericBitmap, Vector2 position, OrderSettingsResult orderSettings)
        {
            CanvasPosition = position;
            Bitmap = genericBitmap;
            TotalPixels = genericBitmap.Pixels.Length;
            GrandTotalSats = orderSettings.PricePerPixel * TotalPixels;
            DimensionString = $"{Bitmap.Width} x {Bitmap.Height}";
        }

        public Vector2 CanvasPosition { get; }
        public GenericBitmap Bitmap { get; }
        public int TotalPixels { get; }
        public int GrandTotalSats { get; }
        public string DimensionString { get; }

        private string _invoice;
        public string Invoice
        {
            get => _invoice;
            set => this.RaiseAndSetIfChanged(ref _invoice, value, nameof(Invoice));
        }

        public OrderPixel[] ToOrderPixel()
        {
            var bW = Bitmap.Width;
            var bH = Bitmap.Height;

            var newArray = new OrderPixel[Bitmap.Pixels.Length];

            for (int x = 0; x < bW; x++)
                for (int y = 0; y < bH; y++)
                {
                    var i = x + bW * y;
                    var cur = new Vector2(x, y);
                    var fin = cur + CanvasPosition;

                    newArray[i] = new OrderPixel(fin, Bitmap.Pixels[i]);
                }

            return newArray;
        }
    }
}
