using Newtonsoft.Json;
using System.Collections.Generic;

namespace VDrumExplorer.InstrumentParser
{
    public class InstrumentGroup
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("veditCategory")]
        public string VEditCategory { get; set; }
        [JsonProperty("instruments")]
        public SortedDictionary<int, string> Instruments { get; set; }
    }
}
