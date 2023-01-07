// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Model.Midi
{
    /// <summary>
    /// Everything required to confidently identify a module, including software revision.
    /// This is used to link schemas with devices and files.
    /// </summary>
    public sealed class ModuleIdentifier : IEquatable<ModuleIdentifier?>
    {
        public static ModuleIdentifier AE01 { get; } = new ModuleIdentifier("AE-01", 0x5a, 0x35a, 0, 0);
        public static ModuleIdentifier AE10 { get; } = new ModuleIdentifier("AE-10", 0x2f, 0x32f, 0, 0x00_00_01_00);
        public static ModuleIdentifier TD07 { get; } = new ModuleIdentifier("TD-07", 0x75, 0x375, 0, 0);
        public static ModuleIdentifier TD17 { get; } = new ModuleIdentifier("TD-17", 0x4b, 0x34b, 0, 0);
        public static ModuleIdentifier TD27 { get; } = new ModuleIdentifier("TD-27", 0x63, 0x363, 0, 0);
        public static ModuleIdentifier TD50 { get; } = new ModuleIdentifier("TD-50", 0x24, 0x324, 0, 0x00_01_00_00);
        public static ModuleIdentifier TD50X { get; } = new ModuleIdentifier("TD-50X", 0x07, 0x407, 0, 0x00_01_00_00);

        /// <summary>
        /// The name of the module, e.g. "TD-17".
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The ID of the module.
        /// </summary>
        public int ModelId { get; }

        /// <summary>
        /// The family code as reported by a Midi identity response.
        /// Note: this is currently interpreted in a little-endian sense, for legacy reasons.
        /// </summary>
        public int FamilyCode { get; }

        /// <summary>
        /// The family number code as reported by a Midi identity response.
        /// Note: this is currently interpreted in a little-endian sense, for legacy reasons.
        /// </summary>
        public int FamilyNumberCode { get; }

        /// <summary>
        /// The length of the model ID in data set/request packets (DT1/RQ1).
        /// This is usually 4, but is 5 for the TD-50X (for no obvious reason).
        /// (This does not contribute to equality checks.)
        /// </summary>
        public int ModelIdLength { get; }

        /// <summary>
        /// The software revision, as reported by a Midi identity response.
        /// (Firmware updates may or may not change this.)
        /// </summary>
        public int SoftwareRevision { get; }

        public ModuleIdentifier(string name, int modelId, int familyCode, int familyNumberCode, int softwareRevision) =>
            (Name, ModelId, FamilyCode, FamilyNumberCode, SoftwareRevision, ModelIdLength) =
            (name, modelId, familyCode, familyNumberCode, softwareRevision, DetermineModelIdLength(name));

        public override bool Equals(object obj) => Equals(obj as ModuleIdentifier);

        public bool Equals(ModuleIdentifier? other) =>
            other != null &&
            (other.Name, other.ModelId, other.FamilyCode, other.FamilyNumberCode, other.SoftwareRevision) == (Name, ModelId, FamilyCode, FamilyNumberCode, SoftwareRevision);

        public override int GetHashCode() => (Name, ModelId, FamilyCode, FamilyNumberCode, SoftwareRevision).GetHashCode();

        public override string ToString() => $"Name: {Name}; ModelId: {ModelId}; FamilyCode: {FamilyCode}; FamilyNumberCode: {FamilyNumberCode}; SoftwareRevision: {SoftwareRevision}";

        /// <summary>
        /// Returns a new instance with all properties the same, other than the given new software revision.
        /// </summary>
        /// <param name="softwareRevision">The new software revision</param>
        public ModuleIdentifier WithSoftwareRevision(int softwareRevision) =>
            new ModuleIdentifier(Name, ModelId, FamilyCode, FamilyNumberCode, softwareRevision);

        // TODO: Make this less hacky.
        private static int DetermineModelIdLength(string name) => name == "TD-50X" ? 5 : 4;
    }
}
