// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

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
        /// The group containing the instrument, or null for a user sample.
        /// </summary>
        public InstrumentGroup? Group { get; }

        /// <summary>
        /// Default values for Vedit fields (e.g. Size), or null if there are no defaults to apply.
        /// </summary>
        public IReadOnlyDictionary<string, int>? DefaultFieldValues { get; }

        private Instrument(int id, string name, InstrumentGroup? group, IReadOnlyDictionary<string, int>? defaultFieldValues) =>
            (Id, Name, Group, DefaultFieldValues) = (id, name, group, defaultFieldValues);

        internal static Instrument FromPreset(int id, string name, InstrumentGroup group, IReadOnlyDictionary<string, int>? defaultFieldValues) =>
            new Instrument(id, name, group, defaultFieldValues);

        internal static Instrument FromUserSample(int id) =>
            new Instrument(id, $"User sample {id + 1}", null, null);

        public override string ToString() => Name;
    }
}
