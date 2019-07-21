using Newtonsoft.Json;
using System;

namespace VDrumExplorer.Models.Json
{
    internal class HexStringConverter : JsonConverter<HexString>
    {
        public override void WriteJson(JsonWriter writer, HexString value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override HexString ReadJson(JsonReader reader, Type objectType, HexString existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            new HexString((string) reader.Value);
    }
}
