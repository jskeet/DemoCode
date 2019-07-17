using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using VDrumExplorer.Models.Fields;

namespace VDrumExplorer.Models
{
    /// <summary>
    /// The data for instruments used within a specific kit.
    /// </summary>
    public sealed class InstrumentFields
    {
        /// <summary>
        /// The (1-based) number of the instrument within the kit.
        /// </summary>
        public int InstrumentNumber { get; }
        public KitFields Kit { get; }
        public IReadOnlyList<FieldSet> FieldSets { get; }

        internal InstrumentFields(KitFields kit, int instrument, IReadOnlyList<FieldSet> fieldSets)
        {
            InstrumentNumber = instrument;
            Kit = kit;
            FieldSets = fieldSets;
        }
    }
}
