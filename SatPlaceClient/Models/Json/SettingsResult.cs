using System;

namespace SatPlaceClient.Models.Json
{
    public class SettingsResult
    {
        public SettingsResult(RawSettingsResult res)
        {
            BoardLength = res.BoardLength;
            InvoiceExpiry = res.InvoiceExpiry;
            OrderPixelsLimit = OrderPixelsLimit;
            PricePerPixel = PricePerPixel;

            var genPixs = new GenericPixel[res.Colors.Length];

            for (int i = 0; i < res.Colors.Length; i++)
            {
                var colorString = res.Colors[i].ToUpperInvariant().TrimStart('#');
                var R = Convert.ToByte(colorString[0..2], 16);
                var G = Convert.ToByte(colorString[2..4], 16);
                var B = Convert.ToByte(colorString[4..6], 16);

                genPixs[i] = new GenericPixel(R, G, B, byte.MaxValue);
            }

            Colors = genPixs;
        }


        public uint BoardLength { get; set; }
        public GenericPixel[] Colors { get; set; }
        public int InvoiceExpiry { get; set; }
        public int OrderPixelsLimit { get; set; }
        public int PricePerPixel { get; set; }
    }
}
