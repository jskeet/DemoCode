// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model
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

        internal InstrumentGroup(string description, int index,
            Dictionary<int, string> instruments,
            Dictionary<string, Dictionary<int, int>>? instrumentFieldDefaults)
        {
            Description = description;
            Index = index;
            Instruments = instruments
                .Select(pair => Instrument.FromPreset(pair.Key, pair.Value, this, BuildInstrumentDefaults(instrumentFieldDefaults, pair.Key)))
                .OrderBy(i => i.Id)
                .ToList()
                .AsReadOnly();
        }

        private IReadOnlyDictionary<string, int>? BuildInstrumentDefaults(
            Dictionary<string, Dictionary<int, int>>? instrumentFieldDefaults, int instrumentId)
        {
            if (instrumentFieldDefaults == null)
            {
                return null;
            }
            var fieldsForInstrument = instrumentFieldDefaults
                .Where(pair => pair.Value.ContainsKey(instrumentId))
                .ToDictionary(pair => pair.Key, pair => pair.Value[instrumentId]);
            return fieldsForInstrument.Count == 0
                ? null
                : fieldsForInstrument.AsReadOnly();
        }

        public override string ToString() => Description;
    }
}
