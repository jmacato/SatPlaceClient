namespace SatPlaceClient.Models
{
    public class GenericBitmap
    {
        public readonly int Width;
        public readonly int Height;
        public readonly GenericColor[] Colors;

        public GenericBitmap(int width, int height, GenericColor[] colors)
        {
            Width = width;
            Height = height;
            Colors = colors;
        }
    }
}