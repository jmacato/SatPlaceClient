using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class PixelVector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, [AllowNull] Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue((int)value.X);
            writer.WriteValue((int)value.Y);
            writer.WriteEndArray();
        }
    }
}
