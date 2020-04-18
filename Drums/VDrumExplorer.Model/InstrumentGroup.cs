// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
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
        /// <summary>
        /// The general description of the group.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Index into <see cref="ModuleSchema.InstrumentGroups"/>.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The instruments in the group
        /// </summary>
        public IReadOnlyList<Instrument> Instruments { get; }

        /// <summary>
        /// True for preset instruments; false for user samples.
        /// </summary>
        public bool Preset { get; }

        /// <summary>
        /// The instrument bank containing this group.
        /// </summary>
        public InstrumentBank Bank => Preset ? InstrumentBank.Preset : InstrumentBank.UserSamples;

        private InstrumentGroup(
            bool preset,
            int index,
            string description,
            Func<InstrumentGroup, IEnumerable<Instrument>> instrumentProvider)
        {
            Preset = preset;
            Description = description;
            Index = index;
            Instruments = instrumentProvider(this).ToReadOnlyList();
        }

        internal static InstrumentGroup ForPresetInstruments(int instrumentGroupIndex, string description,
            Dictionary<int, string> instruments,
            Dictionary<string, Dictionary<int, int>>? instrumentFieldDefaults) =>
            new InstrumentGroup(preset: true, instrumentGroupIndex, description, group => instruments
                .Select(pair => Instrument.FromPreset(pair.Key, pair.Value, group, BuildInstrumentDefaults(instrumentFieldDefaults, pair.Key)))
                .OrderBy(i => i.Id));

        internal static InstrumentGroup ForUserSamples(int instrumentGroupIndex, int sampleCount) =>
            new InstrumentGroup(preset: false, instrumentGroupIndex, "User samples",
                group => Enumerable.Range(0, sampleCount).Select(id => Instrument.FromUserSample(id, group)));

        private static IReadOnlyDictionary<string, int?>? BuildInstrumentDefaults(
            Dictionary<string, Dictionary<int, int>>? instrumentFieldDefaults, int instrumentId)
        {
            if (instrumentFieldDefaults == null)
            {
                return null;
            }
            var fieldsForInstrument = instrumentFieldDefaults
                .ToDictionary(pair => pair.Key, pair => pair.Value.TryGetValue(instrumentId, out var value) ? value : default(int?));
            return fieldsForInstrument.Any() ? fieldsForInstrument.AsReadOnly() : null;
        }

        public override string ToString() => Description;
    }
}
