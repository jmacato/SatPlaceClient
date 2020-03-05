using System;
using System.Numerics;

namespace SatPlaceClient.Models.Json
{
    public class OrderSettingsResult
    {
        public OrderSettingsResult(RawOrderSettingsResult res)
        {
            BoardDimensions = new Vector2(res.BoardLength, res.BoardLength);
            InvoiceExpiry = res.InvoiceExpiry;
            OrderPixelsLimit = res.OrderPixelsLimit;
            PricePerPixel = res.PricePerPixel;

            var genPixs = new GenericColor[res.Colors.Length];

            for (int i = 0; i < res.Colors.Length; i++)
            {
                var colorString = res.Colors[i].ToUpperInvariant().TrimStart('#');
                var R = Convert.ToByte(colorString[0..2], 16);
                var G = Convert.ToByte(colorString[2..4], 16);
                var B = Convert.ToByte(colorString[4..6], 16);

                genPixs[i] = new GenericColor(R, G, B, byte.MaxValue);
            }

            Colors = genPixs;
        }


        public Vector2 BoardDimensions { get; set; }
        public GenericColor[] Colors { get; set; }
        public int InvoiceExpiry { get; set; }
        public int OrderPixelsLimit { get; set; }
        public int PricePerPixel { get; set; }
    }
}
