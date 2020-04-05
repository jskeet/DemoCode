﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;
using VDrumExplorer.Model;

namespace VDrumExplorer.Proto
{
    internal partial class ModuleIdentifier
    {
        internal Model.ModuleIdentifier ToModel() =>
            new Model.ModuleIdentifier(Name, ModelId, FamilyCode, FamilyNumberCode);
        
        internal static ModuleIdentifier FromModel(Model.ModuleIdentifier id) =>
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
            if (!ModuleSchema.KnownSchemas.TryGetValue(modelIdentifier, out var schema))
            {
                throw new InvalidDataException($"No known schema matches identifier {modelIdentifier}");
            }
            return schema.Value;
        }
    }
}
