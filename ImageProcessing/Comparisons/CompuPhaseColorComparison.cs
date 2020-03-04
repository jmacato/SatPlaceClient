using System;

namespace SatPlaceClient.ImageProcessing.Comparisons
{
    /// <summary>
    /// Implements CompuPhase's Weighted Red-mean method of delta-e.
    /// </summary>
    public class CompuPhaseColorComparison : IColorSpaceComparison
    {
        /// <summary>
        /// Calculates the DE2000 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIEDE2000
        /// Correct implementation provided courtesy of Jonathan Hofinger, jaytar42
        /// </summary>
        public double Compare(IColorSpace c1, IColorSpace c2)
        {
            var rgb1 = c1.ToRgb();
            var rgb2 = c2.ToRgb();

            long rmean = ((long)rgb1.R + (long)rgb2.R) / 2;
            long r = (long)rgb1.R - (long)rgb2.R;
            long g = (long)rgb1.G - (long)rgb2.G;
            long b = (long)rgb1.B - (long)rgb2.B;
            return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
        }
    }
}