using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    public abstract class FieldBase
    {
        private static readonly Dictionary<string, Type> typeMapping = new Dictionary<string, Type>
        {
            { "boolean", typeof(BooleanValue) },
            { "range8", typeof(Range8) },
            { "range16", typeof(Range16) },
            { "range32", typeof(Range32) },
            { "string", typeof(StringField) },
            { "volume32", typeof(Volume32Field) },
            { "enum", typeof(EnumField) }
        };
        
        public string Name { get; set; }
        
        /// <summary>
        /// The address in hex ("0xc0" etc)
        /// </summary>
        [JsonProperty("address")]
        public string HexAddress { get; set; }

        /// <summary>
        /// <see cref="HexAddress"/> in numeric form.
        /// </summary>
        [JsonIgnore]
        public int Address => FieldUtilities.ParseHex(HexAddress);

        public abstract FieldValue ParseSysExData(byte[] data);

        // TODO: If values are all 7-bit, how does this end up negative?

        protected int GetInt32Value(byte[] data) =>
            (int) (short) (
                (data[Address] << 12) |
                (data[Address + 1] << 8) |
                (data[Address + 2] << 4) |
                (data[Address + 3] << 0));
        /*
        protected int GetInt32Value(byte[] data) =>
            (data[Address] << 32) |
            (data[Address + 1] << 24) |
            (data[Address + 2] << 16) |
            (data[Address + 3] << 0);*/

        protected short GetInt16Value(byte[] data) =>
            (sbyte) ((data[Address] << 4) | (data[Address + 1] << 0));

        internal static FieldBase FromJson(JObject json)
        {
            string type = json["type"].Value<string>();
            var bclType = typeMapping[type];
            return (FieldBase) json.ToObject(bclType);
        }
    }
}
