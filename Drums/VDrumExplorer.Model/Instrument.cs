// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model
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
        /// The instrument group containing the instrument. (This might be
        /// the group of user samples.)
        /// </summary>
        public InstrumentGroup Group { get; }

        /// <summary>
        /// The instrument bank containing the instrument. This is just a convenience
        /// property for <see cref="InstrumentGroup.Bank"/>.
        /// </summary>
        public InstrumentBank Bank => Group.Bank;

        /// <summary>
        /// Default values for Vedit fields (e.g. Size), or null if there are no defaults to apply.
        /// If the value is for a dictionary entry is null, that means "set to the default for the field"
        /// (but do set the field when the instrument changes)
        /// </summary>
        public IReadOnlyDictionary<string, int?>? DefaultFieldValues { get; }

        private Instrument(int id, string name, InstrumentGroup group, IReadOnlyDictionary<string, int?>? defaultFieldValues) =>
            (Id, Name, Group, DefaultFieldValues) = (id, name, group, defaultFieldValues);

        internal static Instrument FromPreset(int id, string name, InstrumentGroup group,
            IReadOnlyDictionary<string, int?>? defaultFieldValues) =>
            new Instrument(id, name, group, defaultFieldValues);

        internal static Instrument FromUserSample(int id, InstrumentGroup group) =>
            new Instrument(id, $"User sample {id + 1}", group, null);

        public override string ToString() => Name;
    }
}
