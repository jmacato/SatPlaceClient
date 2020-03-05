using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
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
        private static int[] ThresholdMap =
        {
             0, 48, 12, 60,  3, 51, 15, 63,
            32, 16, 44, 28, 35, 19, 47, 31,
             8, 56,  4, 52, 11, 59,  7, 55,
            40, 24, 36, 20, 43, 27, 39, 23,
             2, 50, 14, 62,  1, 49, 13, 61,
            34, 18, 46, 30, 33, 17, 45, 29,
            10, 58,  6, 54,  9, 57,  5, 53,
            42, 26, 38, 22, 41, 25, 37, 21
        };

        private Vector3[] _palette;
        private float[] _paletteLuminance;
        private readonly Vector3 _errMult = new Vector3(0.09f, 0.09f, 0.09f);
        private readonly Vector3 _luminanceCoef = new Vector3(0.299f, 0.587f, 0.114f);

        public float GetLuminance(Vector3 color) => (color * _luminanceCoef).Length();

        public void Dither(int w, int h, ref GenericColor[] pixels, GenericColor[] palette)
        {
            this._palette = palette.Select(x => x.ToVector3())
                                   .ToArray();

            this._paletteLuminance = _palette.Select(x => GetLuminance(x))
                                             .ToArray();

            int CoordsToIndex(int x, int y) => x + w * y;

            var rePix = pixels;

            Parallel.For(0, h, (y) =>
            {
                for (int x = 0; x < w; x++)
                {
                    var i = CoordsToIndex(x, y);
                    var color = rePix[i];
                    var map_value = ThresholdMap[(x & 7) + ((y & 7) << 3)];
                    var plan = DeviseBestMixingPlan(color);

                    rePix[i] = palette[plan[map_value]];
                }
            });

            pixels = rePix;
        }

        private int[] DeviseBestMixingPlan(GenericColor color)
        {
            var result = new int[64];
            var src = color.ToVector3();

            // Error accumulator
            var errAcc = new Vector3();

            for (int i = 0; i < 64; i++)
            {
                // Current temporary value
                var temp = src + errAcc * _errMult;

                // Clamp it in the allowed RGB range
                temp = Vector3.Clamp(temp, Vector3.Zero, Vector3.One);

                // Find the closest color from the palette
                var leastPenalty = float.MaxValue;
                var chosenIndex = i % 16;

                for (int j = 0; j < 16; j++)
                {
                    var chosenColor = _palette[j];
                    var penalty = ColorCompare(chosenColor, temp);

                    if (penalty < leastPenalty)
                    {
                        leastPenalty = penalty;
                        chosenIndex = j;
                    }
                }

                // Add it to candidates and update the error
                result[i] = chosenIndex;
                errAcc += src - _palette[chosenIndex];
            }

            // Sort the colors according to luminance
            return result.OrderBy(x => _paletteLuminance[x]).ToArray();
        }

        private float ColorCompare(Vector3 rgb1, Vector3 rgb2)
        {
            var l1 = GetLuminance(rgb1);
            var l2 = GetLuminance(rgb2);
            var deltaL = l1 - l2;

            float diffR = rgb1.X - rgb2.X,
                  diffG = rgb1.Y - rgb2.Y,
                  diffB = rgb1.Z - rgb2.Z;
            
            return (diffR * diffR * _luminanceCoef.X + diffG * diffG * _luminanceCoef.Y + diffB * diffB * _luminanceCoef.Z) * 0.75f + deltaL * deltaL;
        }
    }
}