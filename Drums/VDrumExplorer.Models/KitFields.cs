using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models
{
    /// <summary>
    /// Data about a single kit within a module.
    /// </summary>
    public sealed class KitFields
    {
        /// <summary>
        /// The (1-based) number of the kit within the module.
        /// </summary>
        public int KitNumber { get; }

        public ModuleFields Module { get; }
        public IReadOnlyList<FieldSet> FieldSets { get; }
        public IReadOnlyList<InstrumentFields> Instruments { get; }

        internal KitFields(ModuleFields module, int kit, IReadOnlyList<FieldSet> fieldSets, Func<KitFields, IReadOnlyList<InstrumentFields>> instrumentsProvider)
        {
            Module = module;
            KitNumber = kit;
            FieldSets = fieldSets;
            Instruments = instrumentsProvider(this);
        }
    }
}
