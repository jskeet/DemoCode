// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Midi;

namespace VDrumExplorer.Model.Schema.Json
{
    /// <summary>
    /// JSON source for <see cref="ModuleIdentifier"/>.
    /// </summary>
    internal sealed class ModuleIdentifierJson
    {
        /// <summary>
        /// The name of the module, e.g. "TD-17".
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The ID of the module.
        /// </summary>
        public HexInt32? ModelId { get; set; }

        /// <summary>
        /// The family code as reported by a Midi identity response.
        /// </summary>
        public HexInt32? FamilyCode { get; set; }

        /// <summary>
        /// The family number code as reported by a Midi identity response.
        /// </summary>
        public HexInt32? FamilyNumberCode { get; set; }

        public ModuleIdentifier ToModuleIdentifier()
        {
            Validation.ValidateNotNull(Name, nameof(Name));
            Validation.ValidateNotNull(ModelId, nameof(ModelId));
            Validation.ValidateNotNull(FamilyCode, nameof(FamilyCode));
            Validation.ValidateNotNull(FamilyNumberCode, nameof(FamilyNumberCode));
            return new ModuleIdentifier(Name, ModelId.Value, FamilyCode.Value, FamilyNumberCode.Value);
        }
    }
}
