// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Proto
{
    internal partial class ModuleIdentifier
    {
        internal Data.ModuleIdentifier ToModel() =>
            new Data.ModuleIdentifier(Name, ModelId, FamilyCode, FamilyNumberCode);
        
        internal static ModuleIdentifier FromModel(Data.ModuleIdentifier id) =>
            new ModuleIdentifier
            {
                Name = id.Name,
                ModelId = id.ModelId,
                FamilyCode = id.FamilyCode,
                FamilyNumberCode = id.FamilyNumberCode
            };
    }
}
