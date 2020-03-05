using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SatPlaceClient.Models
{
    public readonly struct GenericPixel
    {
        [JsonProperty("coordinates")]
        public readonly Vector2 Coordinate;

        [JsonProperty("color")]
        public readonly GenericColor Color;

        public GenericPixel(Vector2 coordinate, GenericColor color)
        {
            Coordinate = coordinate;
            Color = color;
        }
    }
}
