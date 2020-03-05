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
        public int TotalPixels { get; private set; }
        public int GrandTotalSats { get; private set; }
        public string DimensionString { get; private set; }

        public string ToOrderJsonString()
        {
            var v2Conv = new PixelVector2JsonConverter();
            var pixConv = new GenericColorJsonConverter();

            
            
            var k = new GenericPixel(CanvasPosition, Bitmap.Pixels[0]);
            var p = JsonConvert.SerializeObject(k, v2Conv, pixConv);      

            return null;      
        }
    }
}
