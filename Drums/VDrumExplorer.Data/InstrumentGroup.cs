using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// A group of supported instruments within a module.
    /// </summary>
    public class InstrumentGroup
    {
        public string Description { get; }
        /// <summary>
        /// Index into <see cref="ModuleSchema.InstrumentGroups"/>.
        /// </summary>
        public int Index { get; }
        public IReadOnlyList<Instrument> Instruments { get; }

        internal InstrumentGroup(string description, int index, Dictionary<int, string> instruments)
        {
            Description = description;
            Index = index;
            Instruments = instruments
                .Select(pair => new Instrument(pair.Key, pair.Value, this))
                .OrderBy(i => i.Id)
                .ToList()
                .AsReadOnly();
        }
    }
}
