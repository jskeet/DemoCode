// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;

namespace VDrumExplorer.Data.Proto
{
    internal partial class ModuleIdentifier
    {
        internal Midi.ModuleIdentifier ToModel() =>
            new Midi.ModuleIdentifier(Name, ModelId, FamilyCode, FamilyNumberCode);
        
        internal static ModuleIdentifier FromModel(Midi.ModuleIdentifier id) =>
            new ModuleIdentifier
            {
                Name = id.Name,
                ModelId = id.ModelId,
                FamilyCode = id.FamilyCode,
                FamilyNumberCode = id.FamilyNumberCode
            };

        internal ModuleSchema GetSchema()
        {
            var modelIdentifier = ToModel();
            if (!SchemaRegistry.KnownSchemas.TryGetValue(modelIdentifier, out var schema))
            {
                throw new InvalidDataException($"No known schema matches identifier {modelIdentifier}");
            }
            return schema.Value;
        }
    }
}
