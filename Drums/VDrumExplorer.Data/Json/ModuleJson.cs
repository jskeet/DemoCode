using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Data.Json
{
    /// <summary>
    /// JSON representation of module data. This is the root document, effectively.
    /// </summary>
    internal sealed class ModuleJson
    {
        /// <summary>
        /// Developer-oriented comment. Has no effect.
        /// </summary>
        public string Comment { get; set; }

        public string Name { get; set; }
        public HexString MidiId { get; set; }
        public int Kits { get; set; }
        public int InstrumentsPerKit { get; set; }
        
        public int Triggers { get; set; }
        public List<InstrumentGroupJson> InstrumentGroups { get; set; }        
        public List<ContainerJson> Containers { get; set; }
        public VisualTreeNodeJson VisualTree { get; set; }

        internal static ModuleJson FromJson(JObject json)
        {
            var serializer = new JsonSerializer { Converters = { new HexStringConverter() }, MissingMemberHandling = MissingMemberHandling.Error };
            return json.ToObject<ModuleJson>(serializer);
        }
    }
}
