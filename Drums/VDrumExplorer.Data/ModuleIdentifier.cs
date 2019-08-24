// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// Everything required to confidently identify a module. This is
    /// used to link schemas with devices and files.
    /// </summary>
    public sealed class ModuleIdentifier : IEquatable<ModuleIdentifier?>
    {
        public static ModuleIdentifier TD50 { get; } = new ModuleIdentifier("TD-50", 0x24, 0x324, 0);
        public static ModuleIdentifier TD17 { get; } = new ModuleIdentifier("TD-17", 0x4b, 0x34b, 0);

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
        /// </summary>
        public int FamilyCode { get; }

        /// <summary>
        /// The family number code as reported by a Midi identity response.
        /// </summary>
        public int FamilyNumberCode { get; }

        public ModuleIdentifier(string name, int modelId, int familyCode, int familyNumberCode) =>
            (Name, ModelId, FamilyCode, FamilyNumberCode) = (name, modelId, familyCode, familyNumberCode);

        public override bool Equals(object obj) => Equals(obj as ModuleIdentifier);

        public bool Equals(ModuleIdentifier? other) =>
            other != null &&
            (other.Name, other.ModelId, other.FamilyCode, other.FamilyNumberCode) == (Name, ModelId, FamilyCode, FamilyNumberCode);

        public override int GetHashCode() => (Name, ModelId, FamilyCode, FamilyNumberCode).GetHashCode();
    }
}
