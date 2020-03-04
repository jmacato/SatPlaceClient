using System.Linq;
using System.Numerics; 
using SatPlaceClient.Models;

namespace SatPlaceClient.ImageProcessing.Dithering
{
    /// <summary>
    /// Original algorithm is described in US Patent # 6606166 
    /// filed by Adobe® Systems Incorporated for use in 
    /// Adobe® Photoshop®.
    /// 
    /// Patent Applied: 1999-04-30
    /// 
    /// Patent Granted: 2003-08-12
    /// 
    /// Patent Expired: 2019-11-16
    /// 
    /// C# version written by Jumar Macato (2020). 
    /// 
    /// Modified 8x8 Threshold Map written by Joel Yliluoma.
    /// 
    /// Heavily based on https://bisqwit.iki.fi/story/howto/dither/jy/
    /// </summary>
    public class AdobePatternDithering
    {
        /* 8x8 threshold map (note: the patented pattern dithering algorithm uses 4x4) */
        private static int[] ThresholdMap = {
            0,48,12,60, 3,51,15,63,
            32,16,44,28,35,19,47,31,
            8,56, 4,52,11,59, 7,55,
            40,24,36,20,43,27,39,23,
            2,50,14,62, 1,49,13,61,
            34,18,46,30,33,17,45,29,
            10,58, 6,54, 9,57, 5,53,
            42,26,38,22,41,25,37,21 };
 
        private Vector3[] allowedPalette;
        double[] paletteLuminance = new double[16];

        private double ColorCompare(Vector3 rgb1, Vector3 rgb2)
        {
            double luma1 = (rgb1.X * 0.299 + rgb1.Y * 0.587 + rgb1.Z * 0.114);
            double luma2 = (rgb2.X * 0.299 + rgb2.Y * 0.587 + rgb2.Z * 0.114);
            double lumadiff = luma1 - luma2;
            double diffR = rgb1.X - rgb2.X,
                   diffG = rgb1.Y - rgb2.Y,
                   diffB = rgb1.Z - rgb2.Z;

            return (diffR * diffR * 0.299 + diffG * diffG * 0.587 + diffB * diffB * 0.114) * 0.75
                 + lumadiff * lumadiff;
        }

        private int[] DeviseBestMixingPlan(GenericColor color)
        {
            var result = new int[64];
            var src = color.ToVector3();

            var errMult = new Vector3(0.09f, 0.09f, 0.09f);  // Error multiplier
            var errAcc = new Vector3(); // Error accumulator

            for (int i = 0; i < 64; ++i)
            {
                // Current temporary value
                var t = src + errAcc * errMult;

                // Clamp it in the allowed RGB range
                t = Vector3.Clamp(t, Vector3.Zero, Vector3.One);

                // Find the closest color from the palette
                var least_penalty = double.MaxValue;
                var chosen = i % 16;

                for (int j = 0; j < 16; j++)
                {
                    var chosenColor = allowedPalette[j];
                    var penalty = ColorCompare(chosenColor, t);

                    if (penalty < least_penalty)
                    {
                        least_penalty = penalty;
                        chosen = j;
                    }
                }
                // Add it to candidates and update the error
                result[i] = chosen;
                errAcc += src - allowedPalette[chosen];
            }

            // Sort the colors according to luminance
            return result.OrderBy(x => paletteLuminance[x]).ToArray();
        }

        public void Dither(int w, int h, ref GenericColor[] pixels, GenericColor[] palette)
        {
            this.allowedPalette = palette.Select(x => x.ToVector3()).ToArray();

            for (int c = 0; c < 16; ++c)
            {
                var pal = palette[c].ToVector3();
                paletteLuminance[c] = pal.X * 0.299f + pal.Y * 0.587f + pal.Z * 0.114f;
            }

            int CoordsToIndex(int x, int y) => x + w * y;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var i = CoordsToIndex(x, y);
                    GenericColor color = pixels[i];

                    int map_value = ThresholdMap[(x & 7) + ((y & 7) << 3)];

                    var plan = DeviseBestMixingPlan(color);

                    pixels[i] = palette[plan[map_value]];
                }
            }
        }
    }
}