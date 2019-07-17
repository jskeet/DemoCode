using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Models.Fields
{
    /// <summary>
    /// A set of fields, usually loaded from a JSON description.
    /// </summary>
    public class FieldSet
    {
        public string Description { get; }
        public int StartAddress { get; }
        public int Size { get; }
        public IReadOnlyList<FieldBase> Fields { get; }

        public FieldSet(JObject json, string description, int startAddress)
        {
            Size = FieldUtilities.ParseHex(json["size"].Value<string>());
            StartAddress = startAddress;

            Fields = ((JArray) json["fields"])
                .Children()
                .Cast<JObject>()
                .Select(FieldBase.FromJson)
                .ToList()
                .AsReadOnly();
        }
    }
}
