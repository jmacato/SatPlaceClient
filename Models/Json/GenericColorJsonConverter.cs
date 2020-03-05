using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Newtonsoft.Json;

namespace SatPlaceClient.Models.Json
{
    public class GenericColorJsonConverter : JsonConverter<GenericColor>
    {
        public override GenericColor ReadJson(JsonReader reader, Type objectType, [AllowNull] GenericColor existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        private const string hex = "0123456789abcdef";

        public static string ByteToHex(byte b)
        {
            return $"{hex[(int)(b >> 4)]}{hex[(int)(b & 0xF)]}";
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] GenericColor value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"\"#{ByteToHex(value.R)}{ByteToHex(value.G)}{ByteToHex(value.B)}\"");
        }
    }
}
