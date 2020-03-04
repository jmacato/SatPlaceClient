namespace SatPlaceClient.ImageProcessing.Comparisons
{
    /// <summary>
    /// Implements the Adobe Pattern dithering Luma comparison method of delta-e.
    /// </summary>
    public class AdobeLumaColorComparison : IColorSpaceComparison
    {
        /// <summary>
        /// Calculates the DE2000 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIEDE2000
        /// Correct implementation provided courtesy of Jonathan Hofinger, jaytar42
        /// </summary>
        public double Compare(IColorSpace c1, IColorSpace c2)
        {
            var rgb1 = c1.ToRgb();
            var rgb2 = c2.ToRgb();

            double luma1 = (rgb1.R * 299 + rgb1.G * 587 + rgb1.B * 114) / (255.0 * 1000);
            double luma2 = (rgb2.R * 299 + rgb2.G * 587 + rgb2.B * 114) / (255.0 * 1000);
            double lumadiff = luma1 - luma2;
            double diffR = (rgb1.R - rgb2.R) / 255.0, diffG = (rgb1.G - rgb2.G) / 255.0, diffB = (rgb1.B - rgb2.B) / 255.0;
            return (diffR * diffR * 0.299 + diffG * diffG * 0.587 + diffB * diffB * 0.114) * 0.75
                 + lumadiff * lumadiff;
        }
    }
}