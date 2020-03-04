namespace SatPlaceClient.Models
{
    public class GenericBitmap
    {
        public readonly int Width;
        public readonly int Height;
        public GenericColor[] Pixels;

        public GenericBitmap(int width, int height, GenericColor[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }
    }
}