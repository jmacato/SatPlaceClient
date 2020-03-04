using System.Numerics;

namespace SatPlaceClient.Models
{
    public readonly struct GenericPixel
    {
        public readonly Vector2 Coordinate;
        public readonly GenericColor Color;

        public GenericPixel(Vector2 coordinate, GenericColor color)
        {
            Coordinate = coordinate;
            Color = color;
        }
    }
}
