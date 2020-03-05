using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SatPlaceClient.Models
{
    public readonly struct OrderPixel
    {
        [JsonProperty("coordinates")]
        public readonly string[] Coordinate;

        [JsonProperty("color")]
        public readonly string Color;
        
        private const string hex = "0123456789abcdef";

        private static string ByteToHex(byte b)
        {
            return $"{hex[(int)(b >> 4)]}{hex[(int)(b & 0xF)]}";
        }

        public OrderPixel(Vector2 coordinate, GenericColor color)
        {
            Coordinate = new string[] { $"{(int)coordinate.X}", $"{(int)coordinate.Y}" };
            Color = $"#{ByteToHex(color.R)}{ByteToHex(color.G)}{ByteToHex(color.B)}";
        }
    }
}
