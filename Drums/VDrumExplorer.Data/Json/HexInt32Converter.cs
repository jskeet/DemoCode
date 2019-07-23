using Newtonsoft.Json;
using System;

namespace VDrumExplorer.Data.Json
{
    internal class HexInt32Converter : JsonConverter<HexInt32>
    {
        public override void WriteJson(JsonWriter writer, HexInt32 value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override HexInt32 ReadJson(JsonReader reader, Type objectType, HexInt32 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            new HexInt32((string) reader.Value);
    }
}
