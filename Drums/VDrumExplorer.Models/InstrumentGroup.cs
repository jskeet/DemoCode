using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDrumExplorer.Models
{
    /// <summary>
    /// A group of supported instruments within a module.
    /// </summary>
    public class InstrumentGroup
    {
        public ModuleFields Module { get; }
        public string Name { get; }
        public IReadOnlyList<Instrument> Instruments { get; }

        internal InstrumentGroup(ModuleFields module, string name, Dictionary<int, string> instruments)
        {
            Module = module;
            Name = name;
            Instruments = instruments
                .Select(pair => new Instrument(pair.Key, pair.Value, this))
                .OrderBy(i => i.Id)
                .ToList()
                .AsReadOnly();
        }
    }
}
