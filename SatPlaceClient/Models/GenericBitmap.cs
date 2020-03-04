namespace SatPlaceClient.Models
{
    public class GenericBitmap
    {
        public readonly int Width;
        public readonly int Height;
        public readonly GenericColor[] Pixels;

        public GenericBitmap(int width, int height, GenericColor[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }
    }
}