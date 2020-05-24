// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using System;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Json
{
    internal class HexInt32Converter : JsonConverter<HexInt32>
    {
        public override void WriteJson(JsonWriter writer, HexInt32? value, JsonSerializer serializer) =>
            writer.WriteValue(value?.ToString());

        public override HexInt32 ReadJson(JsonReader reader, Type objectType, HexInt32? existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            HexInt32.Parse(Preconditions.AssertNotNull((string?) reader.Value));
    }
}
