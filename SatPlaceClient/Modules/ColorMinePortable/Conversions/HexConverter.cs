using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatPlaceClient.ImageProcessing.Conversions
{
    internal static class HexConverter
    {
        internal static void ToColorSpace(IRgb color, Hex item)
        {
            item.R = ((int)Math.Round(color.R, 0, MidpointRounding.AwayFromZero)).ToString("X2");
            item.G = ((int)Math.Round(color.G, 0, MidpointRounding.AwayFromZero)).ToString("X2");
            item.B = ((int)Math.Round(color.B, 0, MidpointRounding.AwayFromZero)).ToString("X2");
        }

        internal static IRgb ToColor(IHex item) => new Rgb
        ((double)Convert.ToInt32(item.R, 16),
         (double)Convert.ToInt32(item.G, 16),
         (double)Convert.ToInt32(item.B, 16));
    }
}
