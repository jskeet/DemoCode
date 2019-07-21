using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// An instrument supported within a module. This has no settings applied, and isn't necessarily
    /// part of any kit.
    /// </summary>
    public sealed class Instrument
    {
        /// <summary>
        /// The numeric ID of the instrument.
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// The name of the instrument.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The group containing the instrument.
        /// </summary>
        public InstrumentGroup Group { get; }

        internal Instrument(int id, string name, InstrumentGroup group) =>
            (Id, Name, Group) = (id, name, group);
    }
}
